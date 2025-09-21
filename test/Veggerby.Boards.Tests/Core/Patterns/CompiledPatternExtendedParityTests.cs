using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;

using Xunit;

namespace Veggerby.Boards.Tests.Core.Patterns;

/// <summary>
/// Extended parity tests ensuring compiled pattern resolver produces identical results to the legacy visitor
/// for additional pattern varieties (multi-direction, repeatable, unreachable, null).
/// </summary>
public class CompiledPatternExtendedParityTests
{
    private static (TilePath legacy, TilePath compiled) ResolveBoth(Game game, Piece piece, Tile from, Tile to)
    {
        var legacyVisitor = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(game.Board, from, to);
        foreach (var p in piece.Patterns)
        {
            p.Accept(legacyVisitor);
            if (legacyVisitor.ResultPath is not null && legacyVisitor.ResultPath.To.Equals(to))
            {
                break;
            }
        }
        var legacy = legacyVisitor.ResultPath;

        var table = PatternCompiler.Compile(game);
        var resolver = new CompiledPatternResolver(table, game.Board);
        resolver.TryResolve(piece, from, to, out var compiled);
        return (legacy, compiled);
    }


    [Fact]
    public void GivenMultiDirectionPattern_WhenResolvingAlongSecondDirection_ThenCompiledMatchesLegacy()
    {
        // Star: center o with two spokes to x and y
        var o = new Tile("o"); var x = new Tile("x"); var y = new Tile("y");
        var dx = new Direction("ox"); var dy = new Direction("oy");
        var r1 = new TileRelation(o, x, dx); var r2 = new TileRelation(o, y, dy);
        var board = new Board("board-md-2", new[] { r1, r2 });
        var player = new Player("p1");
        var piece = new Piece("piece-2", player, new IPattern[] { new MultiDirectionPattern(new[] { dx, dy }, isRepeatable: false) });
        var game = new Game(board, new[] { player }, new Artifact[] { piece });

        // act (resolve o -> y)
        var (legacy, compiled) = ResolveBoth(game, piece, o, y);

        // assert
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
        compiled.Relations.Count().Should().Be(legacy.Relations.Count());
    }

    [Fact]
    public void GivenRepeatablePattern_WhenResolvingLongestChain_ThenCompiledMatchesLegacy()
    {
        // a -> b -> c -> d linear repeatable
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d");
        var d1 = new Direction("ab"); var d2 = new Direction("bc"); var d3 = new Direction("cd");
        var r1 = new TileRelation(a, b, d1); var r2 = new TileRelation(b, c, d2); var r3 = new TileRelation(c, d, d3);
        // Use single repeatable direction sequence (d1 reused logically); model by multi-direction single dir repeatable
        var board = new Board("board-md-3", new[] { r1, r2, r3 });
        var player = new Player("p1");
        // We want a single direction; but linear uses distinct direction instances. Provide first direction only; since relations require exact direction match, we need unified direction reused.
        var dir = new Direction("step");
        var r1u = new TileRelation(a, b, dir); var r2u = new TileRelation(b, c, dir); var r3u = new TileRelation(c, d, dir);
        var board2 = new Board("board-md-3u", new[] { r1u, r2u, r3u });
        var piece = new Piece("piece-3", player, new IPattern[] { new MultiDirectionPattern(new[] { dir }, isRepeatable: true) });
        var game = new Game(board2, new[] { player }, new Artifact[] { piece });

        var (legacy, compiled) = ResolveBoth(game, piece, a, d);

        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
    }

    [Fact]
    public void GivenUnreachableFixedPattern_WhenResolving_ThenBothNull()
    {
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var d1 = new Direction("ab"); var d2 = new Direction("bc");
        // only second relation missing (b->c) so pattern cannot complete
        var r1 = new TileRelation(a, b, d1);
        var board = new Board("board-unreach-1", new[] { r1 });
        var player = new Player("p1");
        var piece = new Piece("piece-4", player, new IPattern[] { new FixedPattern(new[] { d1, d2 }) });
        var game = new Game(board, new[] { player }, new Artifact[] { piece });

        var (legacy, compiled) = ResolveBoth(game, piece, a, c);
        legacy.Should().BeNull();
        compiled.Should().BeNull();
    }

    [Fact]
    public void GivenNullPattern_WhenResolving_ThenBothNull()
    {
        var a = new Tile("a"); var b = new Tile("b");
        var d1 = new Direction("ab"); var r1 = new TileRelation(a, b, d1);
        var board = new Board("board-null-1", new[] { r1 });
        var player = new Player("p1");
        var piece = new Piece("piece-5", player, new IPattern[] { new NullPattern() });
        var game = new Game(board, new[] { player }, new Artifact[] { piece });

        var (legacy, compiled) = ResolveBoth(game, piece, a, b);
        legacy.Should().BeNull();
        compiled.Should().BeNull();
    }
}