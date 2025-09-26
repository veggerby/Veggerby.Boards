using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;

namespace Veggerby.Boards.Tests.Core.Patterns;

/// <summary>
/// Guards the current compiled pattern scope: only FixedPattern, DirectionPattern and MultiDirectionPattern
/// (mapped to Fixed/Ray/MultiRay) produce compiled entries. AnyPattern and NullPattern MUST remain runtime-only
/// (ignored by the compiler) to preserve future semantic flexibility.
/// </summary>
public class CompiledPatternScopeGuardTests
{
    private static CompiledPiecePatterns CompileFor(params IPattern[] patterns)
    {
        var player = new Player("p");
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c");
        var dAB = new Direction("ab"); var dBC = new Direction("bc");
        var rAB = new TileRelation(a, b, dAB); var rBC = new TileRelation(b, c, dBC);
        var board = new Board("board-scope-guard", new[] { rAB, rBC });
        var piece = new Piece("x", player, patterns);
        var game = new Game(board, new[] { player }, new[] { piece });

        var table = PatternCompiler.Compile(game);
        return table.ByPiece[piece.Id];
    }

    [Fact]
    public void GivenAnyPattern_WhenCompiled_ThenNoCompiledPatternsEmitted()
    {
        // arrange / act
        var compiled = CompileFor(new AnyPattern());
        // assert
        compiled.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void GivenNullPattern_WhenCompiled_ThenNoCompiledPatternsEmitted()
    {
        var compiled = CompileFor(new NullPattern());
        compiled.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void GivenFixedPattern_WhenCompiled_ThenFixedCompiledPatternPresent()
    {
        var dir1 = new Direction("ab"); var dir2 = new Direction("bc");
        var compiled = CompileFor(new FixedPattern(new[] { dir1, dir2 }));
        compiled.Patterns.Should().ContainSingle(p => p.Kind == CompiledPatternKind.Fixed);
    }

    [Fact]
    public void GivenDirectionPattern_WhenCompiled_ThenRayCompiledPatternPresent()
    {
        var dir = new Direction("ab");
        var compiled = CompileFor(new DirectionPattern(dir, isRepeatable: true));
        compiled.Patterns.Should().ContainSingle(p => p.Kind == CompiledPatternKind.Ray && p.IsRepeatable);
    }

    [Fact]
    public void GivenMultiDirectionPattern_WhenCompiled_ThenMultiRayCompiledPatternPresent()
    {
        var d1 = new Direction("ab"); var d2 = new Direction("ac");
        var compiled = CompileFor(new MultiDirectionPattern(new[] { d1, d2 }, isRepeatable: false));
        compiled.Patterns.Should().ContainSingle(p => p.Kind == CompiledPatternKind.MultiRay && !p.IsRepeatable);
    }

    [Fact]
    public void GivenTinyBoard_WithAllSupportedKinds_WhenResolving_ThenCompiledMatchesLegacy()
    {
        // arrange tiny topology: a -> b -> c plus a -> d diagonal like relation for multi-ray shortest path
        var a = new Tile("a"); var b = new Tile("b"); var c = new Tile("c"); var d = new Tile("d");
        var dab = new Direction("ab"); var dbc = new Direction("bc"); var dad = new Direction("ad");
        var rAB = new TileRelation(a, b, dab); var rBC = new TileRelation(b, c, dbc); var rAD = new TileRelation(a, d, dad);
        var board = new Board("board-parity-mix", new[] { rAB, rBC, rAD });
        var player = new Player("p");
        var piece = new Piece("piece-mix", player, new IPattern[]
        {
            new FixedPattern(new[]{ dab, dbc }),                       // a->b->c
            new DirectionPattern(dab, isRepeatable: true),             // ray along a->b chain
            new MultiDirectionPattern(new[]{ dab, dad }, isRepeatable: false) // choose between b or d (single step)
        });
        var game = new Game(board, new[] { player }, new[] { piece });

        // act legacy: resolve a->c (fixed) then a->b (direction) then a->d (multi-ray alternative) using visitor order semantics
        TilePath legacyFixed = null; TilePath legacyRay = null; TilePath legacyMulti = null;
        foreach (var target in new[] { c, b, d })
        {
            var visitor = new ResolveTilePathPatternVisitor(board, a, target);
            foreach (var pat in piece.Patterns)
            {
                pat.Accept(visitor);
                if (visitor.ResultPath is not null && visitor.ResultPath.To.Equals(target)) break;
            }
            if (target.Equals(c)) legacyFixed = visitor.ResultPath;
            else if (target.Equals(b)) legacyRay = visitor.ResultPath;
            else if (target.Equals(d)) legacyMulti = visitor.ResultPath;
        }

        var table = PatternCompiler.Compile(game);
        var shape = Boards.Internal.Layout.BoardShape.Build(board);
        var resolver = new CompiledPatternResolver(table, board, null, shape);

        resolver.TryResolve(piece, a, c, out var compiledFixed).Should().BeTrue();
        resolver.TryResolve(piece, a, b, out var compiledRay).Should().BeTrue();
        resolver.TryResolve(piece, a, d, out var compiledMulti).Should().BeTrue();

        // assert parity on distances & endpoints
        compiledFixed.To.Should().Be(legacyFixed.To); compiledFixed.Distance.Should().Be(legacyFixed.Distance);
        compiledRay.To.Should().Be(legacyRay.To); compiledRay.Distance.Should().Be(legacyRay.Distance);
        compiledMulti.To.Should().Be(legacyMulti.To); compiledMulti.Distance.Should().Be(legacyMulti.Distance);
    }
}