using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks pattern path resolution using legacy visitor vs compiled patterns for representative movement kinds.
/// Does NOT mutate game state (pure path lookup). Provides a micro-signal for IR effectiveness.
/// </summary>
[MemoryDiagnoser]
public class PatternResolutionBenchmark
{
    private Game _gameLegacy = null!;
    private Game _gameCompiled = null!;
    private List<(Piece piece, Tile from, Tile to)> _queries = null!;
    private CompiledPatternResolver _resolver = null!;
    private CompiledPatternResolver _resolverBoardShape = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Construct a small synthetic board with orthogonal and diagonal connections sufficient for rook/bishop/queen/pawn/knight examples.
        var north = new Direction("north");
        var east = new Direction("east");
        var northEast = new Direction("north-east");
        var a = new Tile("a");
        var b = new Tile("b");
        var c = new Tile("c");
        var d = new Tile("d");
        var e = new Tile("e");

        // Chains: a->b->c (east), c->d->e (north), and diagonal a->c (ne), b->d (ne)
        var rels = new List<TileRelation>
        {
            new(a,b,east), new(b,c,east),
            new(c,d,north), new(d,e,north),
            new(a,c,northEast), new(b,d,northEast)
        };

        var board = new Board("pattern-bench-board", rels);
        var player = new Player("pl");

        // Archetype pieces
        var rook = new Piece("rook", player, [new DirectionPattern(east, true), new DirectionPattern(north, true)]);
        var bishop = new Piece("bishop", player, [new DirectionPattern(northEast, true)]);
        var queen = new Piece("queen", player, [new MultiDirectionPattern([east, north, northEast], true)]);
        var pawn = new Piece("pawn", player, [new DirectionPattern(east, false)]); // single step east
        var knight = new Piece("knight", player, [new FixedPattern([east, east, north])]);
        _gameLegacy = new Game(board, [player], [rook, bishop, queen, pawn, knight]);

        // Compiled variant (table + resolver)
        var table = PatternCompiler.Compile(_gameLegacy); // same game reference
        var shape = Internal.Layout.BoardShape.Build(board);
        _resolver = new CompiledPatternResolver(table, board, null, shape); // BoardShape passed but gate off via flag
        _resolverBoardShape = new CompiledPatternResolver(table, board, null, shape);
        _gameCompiled = _gameLegacy; // alias; compilation is stateless aside from table

        _queries =
        [
            (rook, a, c),        // horizontal 2
            (rook, c, e),        // vertical 2
            (bishop, a, c),      // diagonal 2 (repeatable)
            (queen, a, e),       // multi-direction path east-east-north-north (should fail -> expect null)
            (queen, a, c),       // picks east-east or diagonal depending; diagonal shorter => ensure consistent selection
            (pawn, a, c),        // invalid (non-repeatable two step)
            (pawn, a, b),        // single step valid
            (knight, a, d)       // fixed pattern east, east, north
        ];
    }

    [Benchmark(Baseline = true)]
    public int LegacyVisitor()
    {
        var count = 0;
        foreach (var q in _queries)
        {
            var visitor = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(_gameLegacy.Board, q.from, q.to);
            foreach (var p in q.piece.Patterns)
            {
                p.Accept(visitor);
                if (visitor.ResultPath is not null && visitor.ResultPath.To.Equals(q.to))
                {
                    break;
                }
            }

            if (visitor.ResultPath is not null) count++;
        }
        return count;
    }

    [Benchmark]
    public int Compiled()
    {
        var count = 0;
        foreach (var q in _queries)
        {
            if (_resolver.TryResolve(q.piece, q.from, q.to, out var p) && p is not null)
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int Compiled_BoardShapeFastPath()
    {
        var previous = FeatureFlags.EnableBoardShape;
        FeatureFlags.EnableBoardShape = true;

        try
        {
            var count = 0;
            foreach (var q in _queries)
            {
                if (_resolverBoardShape.TryResolve(q.piece, q.from, q.to, out var p) && p is not null)
                {
                    count++;
                }
            }

            return count;
        }
        finally
        {
            FeatureFlags.EnableBoardShape = previous;
        }
    }
}