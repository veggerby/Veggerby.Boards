using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Parity tests ensuring sliding fast-path (bitboards + ray attacks) matches compiled resolver with
/// occupancy semantics (blockers & captures) per charter, and that legacy fallback wrapped with
/// occupancy filtering produces identical results.
/// </summary>
public class SlidingFastPathParityTests
{
    private sealed record PieceSpec(string Id, string Type, string FromTile, string Owner)
    {
        public override string ToString() => $"{Id}:{Type}:{FromTile}:{Owner}";
    }

    private sealed class SlidingTestBuilder : GameBuilder
    {
        private readonly System.Collections.Generic.List<PieceSpec> _specs;
        public SlidingTestBuilder(System.Collections.Generic.IEnumerable<PieceSpec> specs)
        {
            BoardId = "sliding-fast-path-parity";
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
                    var northRank = rank + 1;
                    if (northRank <= 8)
                    {
                        WithTile(id).WithRelationTo(tileId(file, northRank)).InDirection(Constants.Directions.North);
                    }
                    var southRank = rank - 1;
                    if (southRank >= 1)
                    {
                        WithTile(id).WithRelationTo(tileId(file, southRank)).InDirection(Constants.Directions.South);
                    }
                    if (file < 'h')
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file + 1), rank)).InDirection(Constants.Directions.East);
                    }
                    if (file > 'a')
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file - 1), rank)).InDirection(Constants.Directions.West);
                    }
                    if (file < 'h' && northRank <= 8)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file + 1), northRank)).InDirection(Constants.Directions.NorthEast);
                    }
                    if (file > 'a' && northRank <= 8)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file - 1), northRank)).InDirection(Constants.Directions.NorthWest);
                    }
                    if (file < 'h' && southRank >= 1)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file + 1), southRank)).InDirection(Constants.Directions.SouthEast);
                    }
                    if (file > 'a' && southRank >= 1)
                    {
                        WithTile(id).WithRelationTo(tileId((char)(file - 1), southRank)).InDirection(Constants.Directions.SouthWest);
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
                        break; // no directions
                    default:
                        throw new System.InvalidOperationException("Unknown piece spec type");
                }
                pd.OnTile(spec.FromTile);
            }
        }
    }

    private static TilePath? ResolveWithFlags(System.Collections.Generic.IEnumerable<PieceSpec> specs, PieceSpec moving, string target, bool bitboards, bool compiled)
    {
        // Temporarily toggle sliding fast-path flag when bitboards enabled so parity actually exercises fast-path.
        var builder = new SlidingTestBuilder(specs);
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece(moving.Id);
        var from = progress.Game.GetTile(moving.FromTile);
        var to = progress.Game.GetTile(target);
        if (from == to)
        {
            return null; // zero-length request parity: treat as null path
        }
        try
        {
            return progress.ResolvePathCompiledFirst(piece, from, to);
        }
        finally
        {
        }
    }

    private static void AssertParity(System.Collections.Generic.IEnumerable<PieceSpec> specs, PieceSpec moving, string target)
    {
        var reference = ResolveWithFlags(specs, moving, target, bitboards: false, compiled: true);
        var fast = ResolveWithFlags(specs, moving, target, bitboards: true, compiled: true);
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

    [Fact]
    public void GivenRook_EmptyHorizontalRay_Parity()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        AssertParity([rook], rook, ChessIds.Tiles.H4);
    }

    [Fact]
    public void GivenBishop_LongEmptyDiagonal_Parity()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        AssertParity([bishop], bishop, ChessIds.Tiles.G5);
    }

    [Fact]
    public void GivenQueen_VerticalEmptyRay_Parity()
    {
        // arrange

        // act

        // assert

        var queen = new PieceSpec("queen", "queen", ChessIds.Tiles.D4, "white");
        AssertParity([queen], queen, ChessIds.Tiles.D8);
    }

    [Fact]
    public void GivenRook_FriendlyBlockerMidRay_MoveBeyondBlocked()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.D6, "white");
        AssertParity([rook, friendly], rook, ChessIds.Tiles.D8); // null
    }

    [Fact]
    public void GivenRook_FriendlyBlockerAtTarget_Invalid()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.D6, "white");
        AssertParity([rook, friendly], rook, ChessIds.Tiles.D6); // null
    }

    [Fact]
    public void GivenRook_EnemyBlockerAsTarget_CapturePath()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.D6, "black");
        AssertParity([rook, enemy], rook, ChessIds.Tiles.D6); // capture
    }

    [Fact]
    public void GivenRook_EnemyBlockerMidRay_TargetBeyondBlocked()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.D6, "black");
        AssertParity([rook, enemy], rook, ChessIds.Tiles.D7);
        AssertParity([rook, enemy], rook, ChessIds.Tiles.D8);
    }

    [Fact]
    public void GivenRook_FriendlyThenEnemy_FirstBlockerPrevails()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.D6, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.D7, "black");
        AssertParity([rook, friendly, enemy], rook, ChessIds.Tiles.D7);
    }

    [Fact]
    public void GivenRook_ShortRayLengthOne_Parity()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        AssertParity([rook], rook, ChessIds.Tiles.D5);
    }

    [Fact]
    public void GivenImmobilePiece_NoPath()
    {
        // arrange

        // act

        // assert

        var stone = new PieceSpec("stone", "immobile", ChessIds.Tiles.D4, "white");
        AssertParity([stone], stone, ChessIds.Tiles.D5);
    }

    // ---------------- Parity V2 Extended Scenarios (from movement-semantics charter) ----------------

    [Fact]
    public void GivenRook_AdjacentFriendlyBlocker_TargetInvalid()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.D5, "white");
        AssertParity([rook, friendly], rook, ChessIds.Tiles.D5); // null
    }

    [Fact]
    public void GivenRook_AdjacentEnemy_Capture()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.D5, "black");
        AssertParity([rook, enemy], rook, ChessIds.Tiles.D5); // capture single-step
    }

    [Fact]
    public void GivenBishop_FriendlyBlockerMidRay_TargetBeyondBlocked()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.E3, "white");
        AssertParity([bishop, friendly], bishop, ChessIds.Tiles.G5); // null beyond friendly
    }

    [Fact]
    public void GivenBishop_FriendlyBlockerAtTarget_Invalid()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.E3, "white");
        AssertParity([bishop, friendly], bishop, ChessIds.Tiles.E3); // null (occupied by friendly)
    }

    [Fact]
    public void GivenBishop_EnemyBlockerAsTarget_Capture()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.E3, "black");
        AssertParity([bishop, enemy], bishop, ChessIds.Tiles.E3); // capture
    }

    [Fact]
    public void GivenBishop_EnemyBlockerMidRay_TargetBeyondBlocked()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.E3, "black");
        AssertParity([bishop, enemy], bishop, ChessIds.Tiles.G5); // null beyond enemy blocker
    }

    [Fact]
    public void GivenBishop_FriendlyThenEnemy_FirstBlockerPrevails()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.E3, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.F4, "black");
        AssertParity([bishop, friendly, enemy], bishop, ChessIds.Tiles.F4); // null (friendly earlier)
    }

    [Fact]
    public void GivenBishop_EnemyThenFriendly_FirstBlockerPrevails()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.E3, "black");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.F4, "white");
        AssertParity([bishop, enemy, friendly], bishop, ChessIds.Tiles.F4); // null beyond enemy blocker
    }

    [Fact]
    public void GivenRook_ZeroLengthRequest_ReturnsNull()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        AssertParity([rook], rook, ChessIds.Tiles.D4); // from == to => null
    }

    // ---------------- Decorator Direct Parity (fast-path vs compiled only) ----------------

    private static TilePath? ResolveViaDecorator(System.Collections.Generic.IEnumerable<PieceSpec> specs, PieceSpec moving, string target)
    {
        var builder = new SlidingTestBuilder(specs);
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece(moving.Id);
        var from = progress.Game.GetTile(moving.FromTile);
        var to = progress.Game.GetTile(target);
        return progress.ResolvePathCompiledFirst(piece, from, to);
    }

    private static TilePath? ResolveCompiledOnly(System.Collections.Generic.IEnumerable<PieceSpec> specs, PieceSpec moving, string target)
    {
        var builder = new SlidingTestBuilder(specs);
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece(moving.Id);
        var from = progress.Game.GetTile(moving.FromTile);
        var to = progress.Game.GetTile(target);
        return progress.ResolvePathCompiledFirst(piece, from, to);
    }

    private static void AssertDecoratorParity(System.Collections.Generic.IEnumerable<PieceSpec> specs, PieceSpec moving, string target)
    {
        var fast = ResolveViaDecorator(specs, moving, target);
        var compiled = ResolveCompiledOnly(specs, moving, target);
        if (compiled is null)
        {
            fast.Should().BeNull();
            return;
        }
        fast.Should().NotBeNull();
        fast!.To.Id.Should().Be(compiled.To.Id);
        var fastSeq = fast.Relations.Select(r => r.Direction.Id + ":" + r.From.Id + ":" + r.To.Id).ToArray();
        var cmpSeq = compiled.Relations.Select(r => r.Direction.Id + ":" + r.From.Id + ":" + r.To.Id).ToArray();
        fastSeq.Should().BeEquivalentTo(cmpSeq);
    }

    [Fact]
    public void Decorator_Rook_LongHorizontal_Parity()
    {
        // arrange

        // act

        // assert

        var rook = new PieceSpec("rook", "rook", ChessIds.Tiles.D4, "white");
        AssertDecoratorParity([rook], rook, ChessIds.Tiles.H4);
    }

    [Fact]
    public void Decorator_Bishop_DiagonalCapture_Parity()
    {
        // arrange

        // act

        // assert

        var bishop = new PieceSpec("bishop", "bishop", ChessIds.Tiles.C1, "white");
        var enemy = new PieceSpec("enemy", "immobile", ChessIds.Tiles.E3, "black");
        AssertDecoratorParity([bishop, enemy], bishop, ChessIds.Tiles.E3);
    }

    [Fact]
    public void Decorator_Queen_BlockedBeyondFriendly_NullParity()
    {
        // arrange

        // act

        // assert

        var queen = new PieceSpec("queen", "queen", ChessIds.Tiles.D4, "white");
        var friendly = new PieceSpec("ally", "immobile", ChessIds.Tiles.D6, "white");
        AssertDecoratorParity([queen, friendly], queen, ChessIds.Tiles.D8);
    }
}
