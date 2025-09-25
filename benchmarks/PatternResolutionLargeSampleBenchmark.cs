using System;
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
/// Large-sample benchmark (closure metric) enumerating 1000 random (piece, from, to) queries
/// over a synthetic chess-like board to produce aggregate throughput comparison between legacy visitor,
/// compiled resolver, and compiled+fast-path stack.
/// </summary>
[MemoryDiagnoser]
public class PatternResolutionLargeSampleBenchmark
{
    private Game _game = null!;
    private CompiledPatternResolver _compiled = null!;
    private List<(Piece piece, Tile from, Tile to)> _queries = null!;
    private Internal.Layout.BoardShape _shape = null!;

    [Params(1000)] public int Samples { get; set; } = 1000; // kept param for potential future scale exploration

    [GlobalSetup]
    public void Setup()
    {
        // Build 8x8 grid with orthogonal + diagonal relations (subset adequate for typical chess movement shapes).
        var north = new Direction("north"); var south = new Direction("south"); var east = new Direction("east"); var west = new Direction("west");
        var ne = new Direction("ne"); var nw = new Direction("nw"); var se = new Direction("se"); var sw = new Direction("sw");
        var dirs = new[] { north, south, east, west, ne, nw, se, sw };
        var tiles = new List<Tile>(); var rels = new List<TileRelation>();
        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        for (int r = 1; r <= 8; r++) foreach (var f in files) tiles.Add(new Tile($"{f}{r}"));
        Tile T(char f, int r) => tiles[(r - 1) * 8 + (f - 'a')];
        foreach (var f in files)
        {
            for (int r = 1; r <= 8; r++)
            {
                if (r < 8) rels.Add(new TileRelation(T(f, r), T(f, r + 1), north));
                if (r > 1) rels.Add(new TileRelation(T(f, r), T(f, r - 1), south));
                if (f < 'h') rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r), east));
                if (f > 'a') rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r), west));
                if (f < 'h' && r < 8) rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r + 1), ne));
                if (f > 'a' && r < 8) rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r + 1), nw));
                if (f < 'h' && r > 1) rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r - 1), se));
                if (f > 'a' && r > 1) rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r - 1), sw));
            }
        }
        var board = new Board("large-sample-board", rels);
        var white = new Player("white");

        // Representative pieces: rook, bishop, queen (sliders), knight (fixed jump), pawn (single step), king-like (multi-direction non-repeatable) for diversity.
        var rook = new Piece("rook", white, [new DirectionPattern(north, true), new DirectionPattern(east, true), new DirectionPattern(south, true), new DirectionPattern(west, true)]);
        var bishop = new Piece("bishop", white, [new DirectionPattern(ne, true), new DirectionPattern(nw, true), new DirectionPattern(se, true), new DirectionPattern(sw, true)]);
        var queen = new Piece("queen", white, [new MultiDirectionPattern(dirs, true)]);
        var knight = new Piece("knight", white, [new FixedPattern([east, east, north]), new FixedPattern([east, east, south]), new FixedPattern([west, west, north]), new FixedPattern([west, west, south]), new FixedPattern([north, north, east]), new FixedPattern([north, north, west]), new FixedPattern([south, south, east]), new FixedPattern([south, south, west])]);
        var pawn = new Piece("pawn", white, [new DirectionPattern(north, false)]);
        var king = new Piece("king", white, [new MultiDirectionPattern(dirs, false)]);
        var artifacts = new Artifact[] { rook, bishop, queen, knight, pawn, king, white, board };
        _game = new Game(board, [white], artifacts);

        var table = PatternCompiler.Compile(_game);
        _shape = Internal.Layout.BoardShape.Build(board);
        _compiled = new CompiledPatternResolver(table, board, null, _shape);

        // Random query generation (deterministic seed) selecting piece, from, to.
        var rnd = new System.Random(1337);
        var piecePool = new[] { rook, bishop, queen, knight, pawn, king };
        _queries = new List<(Piece, Tile, Tile)>(Samples);
        for (int i = 0; i < Samples; i++)
        {
            var piece = piecePool[rnd.Next(piecePool.Length)];
            var from = tiles[rnd.Next(tiles.Count)];
            var to = tiles[rnd.Next(tiles.Count)];
            if (from == to) { i--; continue; } // enforce non-zero length
            _queries.Add((piece, from, to));
        }
    }

    [Benchmark(Baseline = true)]
    public int LegacyVisitor()
    {
        int hits = 0;
        foreach (var (piece, from, to) in _queries)
        {
            var visitor = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(_game.Board, from, to);
            foreach (var p in piece.Patterns)
            {
                p.Accept(visitor);
                if (visitor.ResultPath is not null && visitor.ResultPath.To.Equals(to)) { hits++; break; }
            }
        }
        return hits;
    }

    [Benchmark]
    public int Compiled()
    {
        int hits = 0;
        foreach (var (piece, from, to) in _queries)
        {
            if (_compiled.TryResolve(piece, from, to, out var path) && path is not null && path.To.Equals(to)) hits++;
        }
        return hits;
    }
}