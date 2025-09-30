using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;

namespace Veggerby.Boards.Tests.Core.Patterns;

public class CompiledDirectionPatternParityTests
{
    private static (TilePath legacy, TilePath compiled) ResolveBoth(Game game, Piece piece, Tile from, Tile to)
    {
        var legacyVisitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
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
        var shape = Boards.Internal.Layout.BoardShape.Build(game.Board);
        var resolver = new CompiledPatternResolver(table, game.Board, null, shape);
        resolver.TryResolve(piece, from, to, out var compiled);

        return (legacy, compiled);
    }

    [Fact]
    public void GivenRepeatableDirectionPattern_WhenResolvingLongestChain_ThenCompiledMatchesLegacy()
    {
        // a -> b -> c (repeatable same direction)
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var dir = new Direction("step");
        var r1 = new TileRelation(a, b, dir); var r2 = new TileRelation(b, c, dir);
        var board = new Board("board-dir-1", [r1, r2]);
        var player = new Player("p1");
        var piece = new Piece("piece-dir-1", player, [new DirectionPattern(dir, isRepeatable: true)]);
        var game = new Game(board, [player], [piece]);

        var (legacy, compiled) = ResolveBoth(game, piece, a, c);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
    }

    [Fact]
    public void GivenNonRepeatableDirectionPattern_WhenResolvingBeyondFirstStep_ThenBothNull()
    {
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var dir = new Direction("step");
        var r1 = new TileRelation(a, b, dir); var r2 = new TileRelation(b, c, dir);
        var board = new Board("board-dir-2", [r1, r2]);
        var player = new Player("p1");
        var piece = new Piece("piece-dir-2", player, [new DirectionPattern(dir, isRepeatable: false)]);
        var game = new Game(board, [player], [piece]);

        var (legacy, compiled) = ResolveBoth(game, piece, a, c); // cannot reach c in one step
        legacy.Should().BeNull();
        compiled.Should().BeNull();
    }

    [Fact]
    public void GivenNonRepeatableDirectionPattern_WhenResolvingSingleStep_ThenCompiledMatchesLegacy()
    {
        var a = new Tile("a"); var b = new Tile("b");
        var dir = new Direction("step");
        var r1 = new TileRelation(a, b, dir);
        var board = new Board("board-dir-3", [r1]);
        var player = new Player("p1");
        var piece = new Piece("piece-dir-3", player, [new DirectionPattern(dir, isRepeatable: false)]);
        var game = new Game(board, [player], [piece]);

        var (legacy, compiled) = ResolveBoth(game, piece, a, b);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
    }
}