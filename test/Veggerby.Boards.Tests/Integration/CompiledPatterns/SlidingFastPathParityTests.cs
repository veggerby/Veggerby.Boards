using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Tests.Utils;

using Xunit;

namespace Veggerby.Boards.Tests.Integration.CompiledPatterns;

/// <summary>
/// Parity tests ensuring the sliding attack fast-path inside <see cref="GameExtensions.ResolvePathCompiledFirst(GameProgress, Piece, Tile, Tile)"/>
/// produces identical geometric paths as the legacy visitor for representative rook / bishop / queen sliders on an empty board.
/// </summary>
/// <remarks>
/// Occupancy-sensitive scenarios (blocked rays, captures) are deferred until semantics are finalized for whether geometric path
/// resolution should ignore blockers (as the legacy visitor does) or respect them (attack generator does). These tests constrain
/// evaluation to empty-ray cases so both implementations must agree.
/// </remarks>
public class SlidingFastPathParityTests
{
    private sealed record PieceSpec(string Id, string Type, string FromTile);

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
            // directions (cardinal + diagonals)
            AddDirection("north"); AddDirection("south"); AddDirection("east"); AddDirection("west");
            AddDirection("north-east"); AddDirection("north-west"); AddDirection("south-east"); AddDirection("south-west");

            // players
            AddPlayer("white"); AddPlayer("black");

            // tiles tile-a1..tile-h8
            var files = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
            for (int rank = 1; rank <= 8; rank++)
            {
                foreach (var file in files)
                {
                    AddTile($"tile-{file}{rank}");
                }
            }

            // relations via fluent API
            System.Func<char, int, string> tileId = (f, r) => $"tile-{f}{r}";
            foreach (var file in files)
            {
                for (int rank = 1; rank <= 8; rank++)
                {
                    var id = tileId(file, rank);
                    var northRank = rank + 1; if (northRank <= 8) { WithTile(id).WithRelationTo(tileId(file, northRank)).InDirection("north"); }
                    var southRank = rank - 1; if (southRank >= 1) { WithTile(id).WithRelationTo(tileId(file, southRank)).InDirection("south"); }
                    if (file < 'h') { WithTile(id).WithRelationTo(tileId((char)(file + 1), rank)).InDirection("east"); }
                    if (file > 'a') { WithTile(id).WithRelationTo(tileId((char)(file - 1), rank)).InDirection("west"); }
                    if (file < 'h' && northRank <= 8) { WithTile(id).WithRelationTo(tileId((char)(file + 1), northRank)).InDirection("north-east"); }
                    if (file > 'a' && northRank <= 8) { WithTile(id).WithRelationTo(tileId((char)(file - 1), northRank)).InDirection("north-west"); }
                    if (file < 'h' && southRank >= 1) { WithTile(id).WithRelationTo(tileId((char)(file + 1), southRank)).InDirection("south-east"); }
                    if (file > 'a' && southRank >= 1) { WithTile(id).WithRelationTo(tileId((char)(file - 1), southRank)).InDirection("south-west"); }
                }
            }

            // piece definitions
            foreach (var spec in _specs)
            {
                var pd = AddPiece(spec.Id).WithOwner("white");
                switch (spec.Type)
                {
                    case "rook":
                        pd.HasDirection("north").CanRepeat()
                            .HasDirection("east").CanRepeat()
                            .HasDirection("south").CanRepeat()
                            .HasDirection("west").CanRepeat();
                        break;
                    case "bishop":
                        pd.HasDirection("north-east").CanRepeat()
                            .HasDirection("north-west").CanRepeat()
                            .HasDirection("south-east").CanRepeat()
                            .HasDirection("south-west").CanRepeat();
                        break;
                    case "queen":
                        pd.HasDirection("north").CanRepeat()
                            .HasDirection("east").CanRepeat()
                            .HasDirection("south").CanRepeat()
                            .HasDirection("west").CanRepeat()
                            .HasDirection("north-east").CanRepeat()
                            .HasDirection("north-west").CanRepeat()
                            .HasDirection("south-east").CanRepeat()
                            .HasDirection("south-west").CanRepeat();
                        break;
                    default: throw new System.InvalidOperationException("Unknown piece spec type");
                }
                pd.OnTile(spec.FromTile);
            }
        }
    }

    private static TilePath ResolveWithFlags(PieceSpec spec, string target, bool bitboards, bool compiled)
    {
        using var scope = new FeatureFlagScope(bitboards: bitboards, compiledPatterns: compiled, boardShape: true); // boardShape always on
        var builder = new SlidingTestBuilder([spec]);
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece(spec.Id);
        var from = progress.Game.GetTile(spec.FromTile);
        var to = progress.Game.GetTile(target);
        return progress.ResolvePathCompiledFirst(piece, from, to);
    }

    private static void AssertParity(PieceSpec spec, string target)
    {
        // reference = compiled patterns only (no bitboards => no sliding fast-path)
        var reference = ResolveWithFlags(spec, target, bitboards: false, compiled: true);
        var fast = ResolveWithFlags(spec, target, bitboards: true, compiled: true); // fast-path eligible
        if (reference is null)
        {
            Assert.Null(fast);
            return;
        }
        Assert.NotNull(fast);
        Assert.Equal(reference.To.Id, fast!.To.Id);
        var fastSeq = fast.Relations.Select(r => r.Direction.Id + ":" + r.From.Id + ":" + r.To.Id).ToArray();
        var refSeq = reference.Relations.Select(r => r.Direction.Id + ":" + r.From.Id + ":" + r.To.Id).ToArray();
        Assert.Equal(refSeq, fastSeq);
    }

    [Fact]
    public void GivenRook_WhenResolvingHorizontalRay_ThenFastPathMatchesLegacy()
    {
        // arrange / act
        AssertParity(new PieceSpec("rook-1", "rook", "tile-d4"), "tile-h4");
        // assert performed in helper
    }

    [Fact]
    public void GivenBishop_WhenResolvingLongDiagonal_ThenFastPathMatchesLegacy()
    {
        AssertParity(new PieceSpec("bishop-1", "bishop", "tile-c1"), "tile-g5");
    }

    [Fact]
    public void GivenQueen_WhenResolvingVerticalRay_ThenFastPathMatchesLegacy()
    {
        AssertParity(new PieceSpec("queen-1", "queen", "tile-d4"), "tile-d8");
    }
}