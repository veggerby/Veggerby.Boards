using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Tests.Utils;

using Xunit;

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
            AddDirection("north"); AddDirection("south"); AddDirection("east"); AddDirection("west");
            AddDirection("north-east"); AddDirection("north-west"); AddDirection("south-east"); AddDirection("south-west");

            AddPlayer("white"); AddPlayer("black");

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

            foreach (var spec in _specs)
            {
                var pd = AddPiece(spec.Id).WithOwner(spec.Owner);
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
                    case "immobile":
                        break; // no directions
                    default: throw new System.InvalidOperationException("Unknown piece spec type");
                }
                pd.OnTile(spec.FromTile);
            }
        }
    }

    private static TilePath ResolveWithFlags(System.Collections.Generic.IEnumerable<PieceSpec> specs, PieceSpec moving, string target, bool bitboards, bool compiled)
    {
        using var scope = new FeatureFlagScope(bitboards: bitboards, compiledPatterns: compiled, boardShape: true);
        var builder = new SlidingTestBuilder(specs);
        var progress = builder.Compile();
        var piece = progress.Game.GetPiece(moving.Id);
        var from = progress.Game.GetTile(moving.FromTile);
        var to = progress.Game.GetTile(target);
        return progress.ResolvePathCompiledFirst(piece, from, to);
    }

    private static void AssertParity(System.Collections.Generic.IEnumerable<PieceSpec> specs, PieceSpec moving, string target)
    {
        var reference = ResolveWithFlags(specs, moving, target, bitboards: false, compiled: true);
        var fast = ResolveWithFlags(specs, moving, target, bitboards: true, compiled: true);
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
    public void GivenRook_EmptyHorizontalRay_Parity()
    {
        var rook = new PieceSpec("rook", "rook", "tile-d4", "white");
        AssertParity([rook], rook, "tile-h4");
    }

    [Fact]
    public void GivenBishop_LongEmptyDiagonal_Parity()
    {
        var bishop = new PieceSpec("bishop", "bishop", "tile-c1", "white");
        AssertParity([bishop], bishop, "tile-g5");
    }

    [Fact]
    public void GivenQueen_VerticalEmptyRay_Parity()
    {
        var queen = new PieceSpec("queen", "queen", "tile-d4", "white");
        AssertParity([queen], queen, "tile-d8");
    }

    [Fact]
    public void GivenRook_FriendlyBlockerMidRay_MoveBeyondBlocked()
    {
        var rook = new PieceSpec("rook", "rook", "tile-d4", "white");
        var friendly = new PieceSpec("ally", "immobile", "tile-d6", "white");
        AssertParity([rook, friendly], rook, "tile-d8"); // null
    }

    [Fact]
    public void GivenRook_FriendlyBlockerAtTarget_Invalid()
    {
        var rook = new PieceSpec("rook", "rook", "tile-d4", "white");
        var friendly = new PieceSpec("ally", "immobile", "tile-d6", "white");
        AssertParity([rook, friendly], rook, "tile-d6"); // null
    }

    [Fact]
    public void GivenRook_EnemyBlockerAsTarget_CapturePath()
    {
        var rook = new PieceSpec("rook", "rook", "tile-d4", "white");
        var enemy = new PieceSpec("enemy", "immobile", "tile-d6", "black");
        AssertParity([rook, enemy], rook, "tile-d6"); // capture
    }

    [Fact]
    public void GivenRook_EnemyBlockerMidRay_TargetBeyondBlocked()
    {
        var rook = new PieceSpec("rook", "rook", "tile-d4", "white");
        var enemy = new PieceSpec("enemy", "immobile", "tile-d6", "black");
        AssertParity([rook, enemy], rook, "tile-d7");
        AssertParity([rook, enemy], rook, "tile-d8");
    }

    [Fact]
    public void GivenRook_FriendlyThenEnemy_FirstBlockerPrevails()
    {
        var rook = new PieceSpec("rook", "rook", "tile-d4", "white");
        var friendly = new PieceSpec("ally", "immobile", "tile-d6", "white");
        var enemy = new PieceSpec("enemy", "immobile", "tile-d7", "black");
        AssertParity([rook, friendly, enemy], rook, "tile-d7");
    }

    [Fact]
    public void GivenRook_ShortRayLengthOne_Parity()
    {
        var rook = new PieceSpec("rook", "rook", "tile-d4", "white");
        AssertParity([rook], rook, "tile-d5");
    }

    [Fact]
    public void GivenImmobilePiece_NoPath()
    {
        var stone = new PieceSpec("stone", "immobile", "tile-d4", "white");
        AssertParity([stone], stone, "tile-d5");
    }
}