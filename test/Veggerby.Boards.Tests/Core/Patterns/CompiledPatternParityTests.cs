using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns; // for PatternCompiler + CompiledPatternResolver

namespace Veggerby.Boards.Tests.Core.Patterns;

public class CompiledPatternParityTests
{
    // Both legacy visitor and compiled resolver may yield no path; model both tuple elements as nullable.
    private static (TilePath? legacy, TilePath? compiled) ResolveBoth(Game game, Piece piece, Tile from, Tile to)
    {
        // legacy
        var legacyVisitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
        var single = piece.Patterns.Single();
        single.Accept(legacyVisitor);
        var legacy = legacyVisitor.ResultPath;

        // compiled (simulate by directly invoking resolver after manual compile)
        var table = PatternCompiler.Compile(game); // compiler now emits fixed + multi-direction patterns
        var shape = Boards.Internal.Layout.BoardShape.Build(game.Board);
        var resolver = new CompiledPatternResolver(table, game.Board, null, shape);
        resolver.TryResolve(piece, from, to, out var compiled);

        return (legacy, compiled);
    }

    [Fact]
    public void GivenFixedPattern_WhenResolving_ThenCompiledMatchesLegacy()
    {
        // arrange
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var d1 = new Direction("ab"); var d2 = new Direction("bc");
        var r1 = new TileRelation(a, b, d1); var r2 = new TileRelation(b, c, d2);
        var board = new Board("board-1", [r1, r2]);
        var player = new Player("pl1");
        var piece = new Piece("p1", player, [new FixedPattern([d1, d2])]);
        var game = new Game(board, [player], [piece]);
        var from = a; var to = c;

        // act
        var (legacy, compiled) = ResolveBoth(game, piece, from, to);

        // assert
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.From.Should().Be(legacy.From);
        compiled.To.Should().Be(legacy.To);
        compiled.Distance.Should().Be(legacy.Distance);
        compiled.Relations.Count().Should().Be(legacy.Relations.Count());
    }

    [Fact]
    public void GivenBoardShapeFastPathToggled_WhenResolvingFixedPattern_ThenPathsRemainIdentical()
    {
        // arrange
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var d1 = new Direction("ab"); var d2 = new Direction("bc");
        var r1 = new TileRelation(a, b, d1); var r2 = new TileRelation(b, c, d2);
        var board = new Board("board-bs-1", [r1, r2]);
        var player = new Player("pl1");
        var piece = new Piece("p1", player, [new FixedPattern([d1, d2])]);
        var game = new Game(board, [player], [piece]);

        TilePath compiledOff;
        using (new Infrastructure.FeatureFlagScope(compiledPatterns: true, adjacencyCache: false))
        {
            var (legacy, compiled) = ResolveBoth(game, piece, a, c);
            legacy.Should().NotBeNull();
            compiled.Should().NotBeNull();
            compiledOff = compiled;
        }

        TilePath compiledOn;
        // Enable board shape fast path within a scope (extended scope supports boardShape flag)
        using (new Infrastructure.FeatureFlagScope(compiledPatterns: true, adjacencyCache: false, boardShape: true))
        {
            var (legacy, compiled) = ResolveBoth(game, piece, a, c);
            legacy.Should().NotBeNull();
            compiled.Should().NotBeNull();
            compiledOn = compiled;
        }

        compiledOn.Distance.Should().Be(compiledOff.Distance);
        compiledOn.To.Should().Be(compiledOff.To);
    }
}

// builder subclass removed for this focused parity test (direct artifact construction suffices)