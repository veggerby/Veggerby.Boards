using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;

namespace Veggerby.Boards.Tests.Core.Patterns;

/// <summary>
/// Parity tests comparing legacy visitor resolution and compiled pattern resolver
/// for representative chess movement archetypes: knight (fixed L), rook (orthogonal slider),
/// bishop (diagonal slider), queen (multi-direction slider superset) and pawn (single direction non-repeatable).
/// </summary>
public class CompiledChessPatternParityTests
{
    private static (TilePath legacy, TilePath compiled) Resolve(Game game, Piece piece, Tile from, Tile to)
    {
        // legacy (search each pattern until target reached)
        var legacyVisitor = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(game.Board, from, to);
        foreach (var p in piece.Patterns)
        {
            p.Accept(legacyVisitor);
            if (legacyVisitor.ResultPath is not null && legacyVisitor.ResultPath.To.Equals(to))
            {
                break;
            }
        }
        var table = PatternCompiler.Compile(game);
        var resolver = new CompiledPatternResolver(table, game.Board, null);
        resolver.TryResolve(piece, from, to, out var compiled);
        return (legacyVisitor.ResultPath, compiled);
    }

    [Fact]
    public void GivenKnightLShape_WhenResolving_ThenCompiledMatchesLegacy()
    {
        // Board shaped minimal to support an L: a -> b (east), then b -> n1 (north), n1 -> n2 (north)
        var north = new Direction("north"); var east = new Direction("east");
        var a = new Tile("a"); var b = new Tile("b"); var n1 = new Tile("n1"); var n2 = new Tile("n2");
        var ab = new TileRelation(a, b, east); var b_n1 = new TileRelation(b, n1, north); var n1_n2 = new TileRelation(n1, n2, north);
        var board = new Board("knight-board", new[] { ab, b_n1, n1_n2 });
        var player = new Player("pl");
        // Knight pattern example: east + north + north
        var knight = new Piece("kn", player, new IPattern[] { new FixedPattern(new[] { east, north, north }) });
        var game = new Game(board, new[] { player }, new Artifact[] { knight });

        var (legacy, compiled) = Resolve(game, knight, a, n2);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
        compiled.To.Should().Be(legacy.To);
    }

    [Fact]
    public void GivenRookLikeSlider_WhenResolvingLongestChain_ThenCompiledMatchesLegacy()
    {
        var north = new Direction("north");
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d");
        var ab = new TileRelation(a, b, north); var bc = new TileRelation(b, c, north); var cd = new TileRelation(c, d, north);
        var board = new Board("rook-board", new[] { ab, bc, cd });
        var player = new Player("pl");
        var rook = new Piece("rk", player, new IPattern[] { new DirectionPattern(north, isRepeatable: true) });
        var game = new Game(board, new[] { player }, new Artifact[] { rook });
        var (legacy, compiled) = Resolve(game, rook, a, d);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
    }

    [Fact]
    public void GivenMultiDirectionDiagonal_WhenResolving_ThenCompiledMatchesLegacy()
    {
        var ne = new Direction("ne");
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var ab = new TileRelation(a, b, ne); var bc = new TileRelation(b, c, ne);
        var board = new Board("bishop-board", new[] { ab, bc });
        var player = new Player("pl");
        var bishop = new Piece("bp", player, new IPattern[] { new MultiDirectionPattern(new[] { ne }, isRepeatable: true) });
        var game = new Game(board, new[] { player }, new Artifact[] { bishop });
        var (legacy, compiled) = Resolve(game, bishop, a, c);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
    }

    [Fact]
    public void GivenQueenMultiRay_WhenResolvingDirectionBranch_ThenCompiledMatchesLegacy()
    {
        var north = new Direction("north"); var east = new Direction("east");
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d");
        // model vertical chain a->b->c, plus side branch from a->d east (queen choosing direction set)
        var ab = new TileRelation(a, b, north); var bc = new TileRelation(b, c, north); var ad = new TileRelation(a, d, east);
        var board = new Board("queen-board", new[] { ab, bc, ad });
        var player = new Player("pl");
        var queen = new Piece("qn", player, new IPattern[] { new MultiDirectionPattern(new[] { north, east }, isRepeatable: true) });
        var game = new Game(board, new[] { player }, new Artifact[] { queen });
        // resolve vertical path two steps
        var (legacy, compiled) = Resolve(game, queen, a, c);
        legacy.Should().NotBeNull();
        compiled.Should().NotBeNull();
        compiled.Distance.Should().Be(legacy.Distance);
    }

    [Fact]
    public void GivenPawnSingleStepNonRepeatable_WhenResolvingBeyondFirstStep_BothNull()
    {
        var north = new Direction("north");
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var ab = new TileRelation(a, b, north); var bc = new TileRelation(b, c, north);
        var board = new Board("pawn-board", new[] { ab, bc });
        var player = new Player("pl");
        var pawn = new Piece("pw", player, new IPattern[] { new DirectionPattern(north, isRepeatable: false) });
        var game = new Game(board, new[] { player }, new Artifact[] { pawn });
        var (legacy, compiled) = Resolve(game, pawn, a, c); // cannot reach in two steps
        legacy.Should().BeNull();
        compiled.Should().BeNull();
    }
}