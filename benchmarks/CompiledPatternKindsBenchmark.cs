using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Micro-benchmark isolating representative compiled pattern kinds (Fixed, Ray, MultiRay) versus the legacy visitor.
/// </summary>
/// <remarks>
/// Goal: Provide a fine-grained signal for each IR shape independent of mixed piece archetypes.
/// The benchmark uses a small synthetic graph that supports straight, diagonal and multi-direction ray expansion.
/// Queries are intentionally short to emphasize dispatch overhead rather than path enumeration length.
/// </remarks>
[MemoryDiagnoser]
public class CompiledPatternKindsBenchmark
{
    private Game _game = null!;
    private Piece _fixedPiece = null!;
    private Piece _rayPiece = null!;
    private Piece _multiRayPiece = null!;
    private CompiledPatternResolver _resolver = null!;
    private List<(Piece piece, Tile from, Tile to)> _queries = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Directions
        var east = new Direction("east");
        var north = new Direction("north");
        var northEast = new Direction("north-east");

        // Tiles (graph shaped like an L plus a diagonal shortcut): a -> b -> c and a -> d -> e (north) and a -> c (ne)
        var a = new Tile("a");
        var b = new Tile("b");
        var c = new Tile("c");
        var d = new Tile("d");
        var e = new Tile("e");
        var relations = new List<TileRelation>
        {
            new(a,b,east), new(b,c,east),
            new(a,d,north), new(d,e,north),
            new(a,c,northEast) // diagonal skip
        };

        var board = new Board("compiled-kinds-board", relations);
        var player = new Player("pl");

        // Pattern archetypes
        _fixedPiece = new Piece("fixed", player, [new FixedPattern([east, east])]); // a->b->c
        _rayPiece = new Piece("ray", player, [new DirectionPattern(east, true)]);   // repeats east
        _multiRayPiece = new Piece("multiray", player, [new MultiDirectionPattern([east, north, northEast], true)]); // any direction repeatable

        _game = new Game(board, [player], [_fixedPiece, _rayPiece, _multiRayPiece]);

        var table = PatternCompiler.Compile(_game);
        var shape = Internal.Layout.BoardShape.Build(board);
        _resolver = new CompiledPatternResolver(table, board, null, shape);

        _queries =
        [
            (_fixedPiece, a, c),       // should succeed exactly
            (_fixedPiece, a, e),       // invalid for fixed pattern
            (_rayPiece, a, c),         // ray two steps east
            (_rayPiece, a, e),         // invalid (different axis)
            (_multiRayPiece, a, c),    // could match east-east or northEast (shorter) -> path expected
            (_multiRayPiece, a, e)     // north then north (via d)
        ];
    }

    [Benchmark(Baseline = true)]
    public int LegacyVisitor()
    {
        int count = 0;
        foreach (var (piece, from, to) in _queries)
        {
            var visitor = new ResolveTilePathPatternVisitor(_game.Board, from, to);
            foreach (var p in piece.Patterns)
            {
                p.Accept(visitor);
                if (visitor.ResultPath is not null && visitor.ResultPath.To.Equals(to))
                {
                    count++; break;
                }
            }
        }
        return count;
    }

    [Benchmark]
    public int Compiled()
    {
        int count = 0;
        foreach (var (piece, from, to) in _queries)
        {
            if (_resolver.TryResolve(piece, from, to, out var path) && path is not null && path.To.Equals(to))
            {
                count++;
            }
        }
        return count;
    }
}