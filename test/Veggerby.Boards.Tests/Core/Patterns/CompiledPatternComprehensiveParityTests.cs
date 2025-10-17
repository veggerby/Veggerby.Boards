using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.Patterns;

/// <summary>
/// Comprehensive parity suite validating compiled pattern resolver vs legacy visitor across
/// mixed topologies, pattern kinds, repeatability, adjacency cache toggles, board shape fast path,
/// and interaction ordering. Focus is exhaustive coverage rather than minimalism.
/// </summary>
public class CompiledPatternComprehensiveParityTests
{
    // Both legacy (visitor) and compiled resolver may legitimately produce no path (null); reflect that in tuple element nullability.
    private static (TilePath? legacy, TilePath? compiled) Resolve(Game game, Piece piece, Tile from, Tile to, bool enableCache = false, bool enableShape = false)
    {
        var legacyVisitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
        foreach (var p in piece.Patterns)
        {
            p.Accept(legacyVisitor);
            if (legacyVisitor.ResultPath is not null && legacyVisitor.ResultPath.To.Equals(to)) { break; }
        }
        var legacy = legacyVisitor.ResultPath;
        var table = PatternCompiler.Compile(game);
        var shape = Boards.Internal.Layout.BoardShape.Build(game.Board);
        var adjacency = enableCache ? Boards.Internal.Compiled.BoardAdjacencyCache.Build(game.Board) : null;
        var resolver = new CompiledPatternResolver(table, game.Board, adjacency, shape);
        resolver.TryResolve(piece, from, to, out var compiled);
        return (legacy, compiled);
    }

    private static (Tile[] tiles, Direction[] dirs, TileRelation[] rels) Line(string idPrefix, int length, string dirId)
    {
        var tiles = Enumerable.Range(0, length).Select(i => new Tile($"{idPrefix}{i}")).ToArray();
        var dir = new Direction(dirId);
        var rels = tiles.Zip(tiles.Skip(1), (a, b) => new TileRelation(a, b, dir)).ToArray();
        return (tiles, new[] { dir }, rels);
    }

    // 1
    [Fact]
    public void FixedPattern_TwoStep_Parity()
    {
        var (tiles, dirs, rels) = Line("f1-", 3, "d");
        var board = new Board("b-f1", rels);
        var player = new Player("p");
        var piece = new Piece("px", player, [new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2]);
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }

    // 2
    [Fact]
    public void FixedPattern_Unreachable_LastStepMissing_BothNull()
    {
        var (tiles, dirs, rels) = Line("f2-", 2, "d"); // only one relation but pattern needs two
        var board = new Board("b-f2", rels);
        var player = new Player("p");
        var piece = new Piece("px", player, [new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles.Length > 2 ? tiles[2] : new Tile("ghost"));
        legacy.Should().BeNull(); compiled.Should().BeNull();
    }

    // 3
    [Fact]
    public void DirectionPattern_Repeatable_LongChain_Parity()
    {
        var (tiles, dirs, rels) = Line("dpr-", 5, "step");
        var board = new Board("b-dpr", rels);
        var player = new Player("p");
        var piece = new Piece("p", player, [new DirectionPattern(dirs[0], true)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[4]);
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }

    // 4
    [Fact]
    public void DirectionPattern_NonRepeatable_SecondStepNull()
    {
        var (tiles, dirs, rels) = Line("dpn-", 3, "step");
        var board = new Board("b-dpn", rels);
        var player = new Player("p");
        var piece = new Piece("p", player, [new DirectionPattern(dirs[0], false)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2]);
        legacy.Should().BeNull(); compiled.Should().BeNull();
    }

    // 5
    [Fact]
    public void MultiDirection_SingleRepeatable_DistanceParity()
    {
        var (tiles, dirs, rels) = Line("mdr-", 4, "step");
        var board = new Board("b-mdr", rels);
        var player = new Player("p");
        var piece = new Piece("p", player, [new MultiDirectionPattern([dirs[0]], true)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[3]);
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }

    // 6
    [Fact]
    public void MultiDirection_TwoBranches_ShortestSelected()
    {
        // topology: a->b->c->d and a->e (shortcut). multi-direction repeatable chooses shortest a->e vs a->d
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d"); var e = new Tile("e");
        var dir = new Direction("fwd"); var shortDir = new Direction("jump");
        var rAB = new TileRelation(a, b, dir); var rBC = new TileRelation(b, c, dir); var rCD = new TileRelation(c, d, dir); var rAE = new TileRelation(a, e, shortDir);
        var board = new Board("b-md-short", [rAB, rBC, rCD, rAE]);
        var player = new Player("p");
        var piece = new Piece("p", player, [new MultiDirectionPattern([dir, shortDir], true)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, a, e);
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }

    // 7
    [Fact]
    public void AnyPattern_IgnoredByCompiler_LegacyProducesPathCompiledNull()
    {
        var (tiles, dirs, rels) = Line("any-", 2, "d");
        var board = new Board("b-any", rels);
        var player = new Player("p");
        var piece = new Piece("p", player, [new AnyPattern()]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[1]);
        legacy.Should().NotBeNull(); // legacy visitor resolves shortest path via Johnson's algorithm
        compiled.Should().BeNull(); // compiler ignores AnyPattern
    }

    // 8
    [Fact]
    public void NullPattern_IgnoredByCompiler_BothNull()
    {
        var (tiles, dirs, rels) = Line("null-", 2, "d");
        var board = new Board("b-null", rels);
        var player = new Player("p");
        var piece = new Piece("p", player, [new NullPattern()]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[1]);
        legacy.Should().BeNull(); compiled.Should().BeNull();
    }

    // 9
    [Fact]
    public void FixedPattern_WithAdjacencyCache_Parity()
    {
        var (tiles, dirs, rels) = Line("fca-", 3, "d");
        var board = new Board("b-fca", rels);
        var player = new Player("p");
        var piece = new Piece("pfca", player, [new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2], enableCache: true);
            legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
        }
    }

    // 10
    [Fact]
    public void FixedPattern_WithBoardShapeFastPath_Parity()
    {
        var (tiles, dirs, rels) = Line("fshape-", 3, "d");
        var board = new Board("b-fshape", rels);
        var player = new Player("p");
        var piece = new Piece("pfshape", player, [new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        using (new FeatureFlagScope(compiledPatterns: true, boardShape: true))
        {
            var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2], enableShape: true);
            legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
        }
    }

    // 11
    [Fact]
    public void DirectionPattern_WithCache_Parity()
    {
        var (tiles, dirs, rels) = Line("dcache-", 4, "d");
        var board = new Board("b-dcache", rels);
        var player = new Player("p");
        var piece = new Piece("pdc", player, [new DirectionPattern(dirs[0], true)]);
        var game = new Game(board, [player], [piece]);
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[3], enableCache: true);
            legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
        }
    }

    // 12
    [Fact]
    public void MultiDirection_WithCache_Parity()
    {
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d");
        var d1 = new Direction("d1"); var d2 = new Direction("d2");
        var rAB = new TileRelation(a, b, d1); var rAC = new TileRelation(a, c, d2); var rCD = new TileRelation(c, d, d2);
        var board = new Board("b-mc", [rAB, rAC, rCD]);
        var player = new Player("p");
        var piece = new Piece("pmc", player, [new MultiDirectionPattern([d1, d2], true)]);
        var game = new Game(board, [player], [piece]);
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var (legacy, compiled) = Resolve(game, piece, a, d, enableCache: true);
            legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
        }
    }

    // 13
    [Fact]
    public void DirectionPattern_FastPathAndCache_Parity()
    {
        var (tiles, dirs, rels) = Line("dall-", 5, "d");
        var board = new Board("b-dall", rels);
        var player = new Player("p");
        var piece = new Piece("pdall", player, [new DirectionPattern(dirs[0], true)]);
        var game = new Game(board, [player], [piece]);
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true, boardShape: true))
        {
            var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[4], enableCache: true, enableShape: true);
            legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
        }
    }

    // 14
    [Fact]
    public void MultiDirection_FastPath_Parity()
    {
        var (tiles, dirs, rels) = Line("mfshape-", 4, "d");
        var board = new Board("b-mfshape", rels);
        var player = new Player("p");
        var piece = new Piece("pmfshape", player, [new MultiDirectionPattern([dirs[0]], true)]);
        var game = new Game(board, [player], [piece]);
        using (new FeatureFlagScope(compiledPatterns: true, boardShape: true))
        {
            var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[3], enableShape: true);
            legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
        }
    }

    // 15
    [Fact]
    public void FixedPattern_CacheVsNoCache_DistancesEqual()
    {
        var (tiles, dirs, rels) = Line("fcv-", 3, "d");
        var board = new Board("b-fcv", rels);
        var player = new Player("p");
        var piece = new Piece("pfcv", player, [new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        var (legacy1, compiled1) = Resolve(game, piece, tiles[0], tiles[2]);
        legacy1.Should().NotBeNull();
        compiled1.Should().NotBeNull();
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var (legacy2, compiled2) = Resolve(game, piece, tiles[0], tiles[2], enableCache: true);
            legacy2.Should().NotBeNull();
            compiled2.Should().NotBeNull();
            compiled1!.Distance.Should().Be(compiled2!.Distance);
            legacy1!.Distance.Should().Be(legacy2!.Distance);
        }
    }

    // 16
    [Fact]
    public void MultiDirection_ShortestSelection_ConsistencyWithCache()
    {
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d");
        var d1 = new Direction("d1"); var d2 = new Direction("d2");
        var rAB = new TileRelation(a, b, d1); var rAC = new TileRelation(a, c, d2); var rCD = new TileRelation(c, d, d2); var rBD = new TileRelation(b, d, d1);
        var board = new Board("b-mcc", [rAB, rAC, rCD, rBD]);
        var player = new Player("p");
        var piece = new Piece("pmcc", player, [new MultiDirectionPattern([d1, d2], true)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, a, d);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var (legacyCached, compiledCached) = Resolve(game, piece, a, d, enableCache: true);
            legacyCached.Should().NotBeNull();
            compiledCached.Should().NotBeNull();
            compiledCached!.Distance.Should().Be(compiled!.Distance);
            legacyCached!.Distance.Should().Be(legacy!.Distance);
        }
    }

    // 17
    [Fact]
    public void DirectionPattern_CacheDoesNotChangeReachability()
    {
        var (tiles, dirs, rels) = Line("dre-", 3, "d");
        var board = new Board("b-dre", rels);
        var player = new Player("p");
        var piece = new Piece("pdre", player, [new DirectionPattern(dirs[0], false)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2]); // unreachable due non-repeatable
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var (legacyCached, compiledCached) = Resolve(game, piece, tiles[0], tiles[2], enableCache: true);
            legacy.Should().BeNull(); compiled.Should().BeNull(); legacyCached.Should().BeNull(); compiledCached.Should().BeNull();
        }
    }

    // 18
    [Fact]
    public void MultiDirection_CacheDoesNotChangeShortestChoice()
    {
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var dir1 = new Direction("d1"); var dir2 = new Direction("d2");
        var rAB = new TileRelation(a, b, dir1); var rAC = new TileRelation(a, c, dir2);
        var board = new Board("b-mdchoice", [rAB, rAC]);
        var player = new Player("p");
        var piece = new Piece("pmchoice", player, [new MultiDirectionPattern([dir1, dir2], false)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, a, b);
        using (new FeatureFlagScope(compiledPatterns: true, adjacencyCache: true))
        {
            var (legacyCached, compiledCached) = Resolve(game, piece, a, b, enableCache: true);
            compiledCached!.Distance.Should().Be(compiled!.Distance);
            legacyCached!.Distance.Should().Be(legacy!.Distance);
        }
    }

    // 19
    [Fact]
    public void FixedPattern_FastPathDoesNotAlterResult()
    {
        var (tiles, dirs, rels) = Line("ffp-", 3, "d");
        var board = new Board("b-ffp", rels);
        var player = new Player("p");
        var piece = new Piece("pffp", player, [new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2]);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        using (new FeatureFlagScope(compiledPatterns: true, boardShape: true))
        {
            var (legacyFast, compiledFast) = Resolve(game, piece, tiles[0], tiles[2], enableShape: true);
            legacyFast.Should().NotBeNull();
            compiledFast.Should().NotBeNull();
            compiledFast!.Distance.Should().Be(compiled!.Distance);
            legacyFast!.Distance.Should().Be(legacy!.Distance);
        }
    }

    // 20
    [Fact]
    public void DirectionPattern_FastPathDoesNotAlterResult()
    {
        var (tiles, dirs, rels) = Line("dfp-", 4, "d");
        var board = new Board("b-dfp", rels);
        var player = new Player("p");
        var piece = new Piece("pdfp", player, [new DirectionPattern(dirs[0], true)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[3]);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        using (new FeatureFlagScope(compiledPatterns: true, boardShape: true))
        {
            var (legacyFast, compiledFast) = Resolve(game, piece, tiles[0], tiles[3], enableShape: true);
            legacyFast.Should().NotBeNull();
            compiledFast.Should().NotBeNull();
            compiledFast!.Distance.Should().Be(compiled!.Distance);
            legacyFast!.Distance.Should().Be(legacy!.Distance);
        }
    }

    // 21
    [Fact]
    public void MultiDirection_FastPathDoesNotAlterShortest()
    {
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d");
        var d1 = new Direction("d1"); var d2 = new Direction("d2");
        var rAB = new TileRelation(a, b, d1); var rAC = new TileRelation(a, c, d2); var rCD = new TileRelation(c, d, d2); var rBD = new TileRelation(b, d, d1);
        var board = new Board("b-mff", [rAB, rAC, rCD, rBD]);
        var player = new Player("p");
        var piece = new Piece("pmff", player, [new MultiDirectionPattern([d1, d2], true)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, a, d);
        using (new FeatureFlagScope(compiledPatterns: true, boardShape: true))
        {
            var (legacyFast, compiledFast) = Resolve(game, piece, a, d, enableShape: true);
            compiledFast!.Distance.Should().Be(compiled!.Distance);
            legacyFast!.Distance.Should().Be(legacy!.Distance);
        }
    }

    // 22
    [Fact]
    public void MixedPatterns_FirstMatchingDeterminesPath()
    {
        var (tiles, dirs, rels) = Line("mix-", 3, "d");
        var altDir = new Direction("alt");
        var extra = new TileRelation(tiles[0], new Tile("z"), altDir);
        var board = new Board("b-mix", rels.Append(extra));
        var player = new Player("p");
        var piece = new Piece("pmix", player, [new DirectionPattern(dirs[0], false), new FixedPattern([dirs[0], dirs[0]])]); // first pattern only one step
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2]); // only reachable via second pattern; first fails
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }

    // 23
    [Fact]
    public void MixedPatterns_AnyPatternFirst_Ignored_SecondProvidesPath()
    {
        var (tiles, dirs, rels) = Line("mix2-", 3, "d");
        var board = new Board("b-mix2", rels);
        var player = new Player("p");
        var piece = new Piece("pmix2", player, [new AnyPattern(), new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2]);
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }

    // 24
    [Fact]
    public void MixedPatterns_NullPatternFirst_Ignored()
    {
        var (tiles, dirs, rels) = Line("mix3-", 3, "d");
        var board = new Board("b-mix3", rels);
        var player = new Player("p");
        var piece = new Piece("pmix3", player, [new NullPattern(), new FixedPattern([dirs[0], dirs[0]])]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[2]);
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }

    // 25
    [Fact]
    public void MixedPatterns_DirectionThenRepeatableRay_TargetViaRay()
    {
        var (tiles, dirs, rels) = Line("mix4-", 4, "d");
        var board = new Board("b-mix4", rels);
        var player = new Player("p");
        var piece = new Piece("pmix4", player, [new DirectionPattern(dirs[0], false), new DirectionPattern(dirs[0], true)]);
        var game = new Game(board, [player], [piece]);
        var (legacy, compiled) = Resolve(game, piece, tiles[0], tiles[3]);
        legacy.Should().NotBeNull(); compiled.Should().NotBeNull(); compiled.Distance.Should().Be(legacy.Distance);
    }
}