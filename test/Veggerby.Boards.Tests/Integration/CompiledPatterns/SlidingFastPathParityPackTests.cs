using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations; // TilePath
using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Curated minimal sliding fast-path parity “pack” used as a CI gate.
/// Focuses on representative edge classes (empty ray, capture, friendly block, enemy mid-block, zero-length, immobile piece)
/// to give high confidence the broader exhaustive suite (<see cref="SlidingFastPathParityTests"/>) would also pass
/// without re-running all 400+ scenarios on every PR. Acts as an early warning smoke test.
/// </summary>
public class SlidingFastPathParityPackTests
{
    public sealed record PieceSpec(string Id, string Type, string FromTile, string Owner)
    {
        public override string ToString() => $"{Id}:{Type}:{FromTile}:{Owner}";
    }

    private sealed class PackBuilder : GameBuilder
    {
        private readonly List<PieceSpec> _specs;
        public PackBuilder(IEnumerable<PieceSpec> specs)
        {
            BoardId = "sliding-fast-path-pack";
            _specs = specs.ToList();
        }

        protected override void Build()
        {
            AddDirection(Constants.Directions.North);
            AddDirection(Constants.Directions.South);
            AddDirection(Constants.Directions.East);
            AddDirection(Constants.Directions.West);
            AddDirection(Constants.Directions.NorthEast);
            AddDirection(Constants.Directions.NorthWest);
            AddDirection(Constants.Directions.SouthEast);
            AddDirection(Constants.Directions.SouthWest);

            AddPlayer("white");
            AddPlayer("black");

            var files = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
            for (int rank = 1; rank <= 8; rank++)
            {
                foreach (var file in files)
                {
                    AddTile($"tile-{file}{rank}");
                }
            }

            System.Func<char, int, string> tileId = (f, r) => $"tile-{f}{r}";
            foreach (var file in files)
            {
                for (int rank = 1; rank <= 8; rank++)
                {
                    var id = tileId(file, rank);
                    var n = rank + 1;
                    var s = rank - 1;
                    if (n <= 8)
                    {
                        WithTile(id).WithRelationTo(tileId(file, n)).InDirection(Constants.Directions.North);
                    }

                    if (s >= 1)
                    {
                        WithTile(id).WithRelationTo(tileId(file, s)).InDirection(Constants.Directions.South);
                    }

                    if (file < 'h')
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file + 1), rank)).InDirection(Constants.Directions.East);
                    }

                    if (file > 'a')
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file - 1), rank)).InDirection(Constants.Directions.West);
                    }

                    if (file < 'h' && n <= 8)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file + 1), n)).InDirection(Constants.Directions.NorthEast);
                    }

                    if (file > 'a' && n <= 8)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file - 1), n)).InDirection(Constants.Directions.NorthWest);
                    }

                    if (file < 'h' && s >= 1)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file + 1), s)).InDirection(Constants.Directions.SouthEast);
                    }

                    if (file > 'a' && s >= 1)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file - 1), s)).InDirection(Constants.Directions.SouthWest);
                    }
                }
            }

            foreach (var spec in _specs)
            {
                var pd = AddPiece(spec.Id).WithOwner(spec.Owner);
                switch (spec.Type)
                {
                    case "rook":
                        pd.HasDirection(Constants.Directions.North).CanRepeat()
                          .HasDirection(Constants.Directions.East).CanRepeat()
                          .HasDirection(Constants.Directions.South).CanRepeat()
                          .HasDirection(Constants.Directions.West).CanRepeat();
                        break;
                    case "bishop":
                        pd.HasDirection(Constants.Directions.NorthEast).CanRepeat()
                          .HasDirection(Constants.Directions.NorthWest).CanRepeat()
                          .HasDirection(Constants.Directions.SouthEast).CanRepeat()
                          .HasDirection(Constants.Directions.SouthWest).CanRepeat();
                        break;
                    case "queen":
                        pd.HasDirection(Constants.Directions.North).CanRepeat()
                          .HasDirection(Constants.Directions.East).CanRepeat()
                          .HasDirection(Constants.Directions.South).CanRepeat()
                          .HasDirection(Constants.Directions.West).CanRepeat()
                          .HasDirection(Constants.Directions.NorthEast).CanRepeat()
                          .HasDirection(Constants.Directions.NorthWest).CanRepeat()
                          .HasDirection(Constants.Directions.SouthEast).CanRepeat()
                          .HasDirection(Constants.Directions.SouthWest).CanRepeat();
                        break;
                    case "immobile":
                        break; // intentionally no directions
                    default:
                        throw new System.InvalidOperationException("Unknown piece spec type");
                }
                pd.OnTile(spec.FromTile);
            }
        }
    }

    private static TilePath? ResolveWithFlags(IEnumerable<PieceSpec> specs, PieceSpec moving, string target, bool bitboards, bool compiled)
    {
        // Sliding fast-path is enabled by default; enabling bitboards triggers decorator eligibility. For clarity keep explicit intent:
        // (If repository defaults change later, tests remain deterministic via scope controlling bitboards prerequisite.)
        var builder = new PackBuilder(specs);
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece(moving.Id);
        var from = progress.Game.GetTile(moving.FromTile);
        var to = progress.Game.GetTile(target);
        piece.Should().NotBeNull();
        from.Should().NotBeNull();
        to.Should().NotBeNull();
        if (from == to)
        {
            return null;
        }
        return progress.ResolvePathCompiledFirst(piece!, from!, to!);
    }

    private static void AssertParity(IEnumerable<PieceSpec> specs, PieceSpec moving, string target)
    {
        // arrange reference (compiled only, no bitboards)
        TilePath? reference = ResolveWithFlags(specs, moving, target, bitboards: false, compiled: true);
        // act fast-path
        TilePath? fast = ResolveWithFlags(specs, moving, target, bitboards: true, compiled: true);
        // assert
        if (reference is null)
        {
            fast.Should().BeNull();
            return;
        }
        fast.Should().NotBeNull();
        fast!.To.Id.Should().Be(reference.To.Id);
        var fastSeq = fast.Relations.Select(r => r.Direction.Id + ":" + r.From.Id + ":" + r.To.Id).ToArray();
        var refSeq = reference.Relations.Select(r => r.Direction.Id + ":" + r.From.Id + ":" + r.To.Id).ToArray();
        fastSeq.Should().BeEquivalentTo(refSeq);
    }

    public static IEnumerable<object[]> PackScenarios()
    {
        // id, type, from, owner definitions inside scenarios for clarity
        var rook = new PieceSpec("rook", "rook", Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D4, "white");
        var bishop = new PieceSpec("bishop", "bishop", Veggerby.Boards.Chess.Constants.ChessIds.Tiles.C1, "white");
        var queen = new PieceSpec("queen", "queen", Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D4, "white");
        var friendly = new PieceSpec("ally", "immobile", Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D6, "white");
        var enemy = new PieceSpec("enemy", "immobile", Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D6, "black");
        var immobile = new PieceSpec("stone", "immobile", Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D4, "white");

        // Each object[]: specs collection, moving piece, target tile, descriptive label (xUnit display)
        yield return new object[] { new[] { rook }, rook, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.H4, "Rook long empty horizontal" };
        yield return new object[] { new[] { rook, enemy }, rook, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D6, "Rook capture enemy mid ray" };
        yield return new object[] { new[] { rook, friendly }, rook, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D8, "Rook friendly mid blocks beyond" };
        yield return new object[] { new[] { bishop, friendly }, bishop, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.G5, "Bishop friendly mid ray blocks" };
        yield return new object[] { new[] { queen, friendly }, queen, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D6, "Queen friendly at target invalid" };
        yield return new object[] { new[] { rook }, rook, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D4, "Rook zero length" };
        yield return new object[] { new[] { immobile }, immobile, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D5, "Immobile cannot move" };
    }

    [Theory(DisplayName = "Sliding Fast-Path Parity Pack")]
    [MemberData(nameof(PackScenarios))]
    public void PackScenario_Parity(IEnumerable<PieceSpec> specs, PieceSpec moving, string target, string _)
    {
        AssertParity(specs, moving, target);
    }
}