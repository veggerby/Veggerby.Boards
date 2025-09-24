using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Compiled;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks sliding path resolution (rook-style and bishop-style rays) under varying occupancies
/// comparing: legacy visitor, compiled resolver only, and compiled+bitboards fast-path.
/// </summary>
[MemoryDiagnoser]
public class SlidingPathResolutionBenchmark
{
    private Game _game = null!;
    private CompiledPatternResolver _compiled = null!;
    private BoardShape _shape = null!;
    private List<(Piece piece, Tile from, Tile to)> _queriesEmpty = null!;
    private List<(Piece piece, Tile from, Tile to)> _queriesQuarter = null!;
    private List<(Piece piece, Tile from, Tile to)> _queriesHalf = null!;

    [Params("empty", "quarter", "half")] public string Density { get; set; } = "empty";

    [GlobalSetup]
    public void Setup()
    {
        // Build full 8x8 grid (tile-a1..tile-h8) with orthogonal + diagonal directions
        var north = new Direction("north"); var south = new Direction("south"); var east = new Direction("east"); var west = new Direction("west");
        var ne = new Direction("north-east"); var nw = new Direction("north-west"); var se = new Direction("south-east"); var sw = new Direction("south-west");
        var dirs = new[] { north, south, east, west, ne, nw, se, sw };
        var tiles = new List<Tile>();
        var rels = new List<TileRelation>();
        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        for (int r = 1; r <= 8; r++)
        {
            foreach (var f in files) tiles.Add(new Tile($"tile-{f}{r}"));
        }
        Tile T(char f, int r) => tiles.Single(t => t.Id == $"tile-{f}{r}");
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
        var board = new Board("bench-sliding-8x8", rels);
        var white = new Player("white"); var black = new Player("black");
        // Slider archetypes
        var rook = new Piece("rook", white, new IPattern[] { new DirectionPattern(north, true), new DirectionPattern(east, true), new DirectionPattern(south, true), new DirectionPattern(west, true) });
        var bishop = new Piece("bishop", white, new IPattern[] { new DirectionPattern(ne, true), new DirectionPattern(nw, true), new DirectionPattern(se, true), new DirectionPattern(sw, true) });
        var queen = new Piece("queen", white, new IPattern[] { new MultiDirectionPattern(dirs, true) });
        // Populate all pieces (movers + blockers) into artifact list
        var artifacts = new List<Artifact> { rook, bishop, queen, white, black, board };
        _game = new Game(board, new[] { white, black }, artifacts);

        var table = PatternCompiler.Compile(_game);
        _shape = BoardShape.Build(board);
        _compiled = new CompiledPatternResolver(table, board, null, _shape);

        // Queries: representative rays (orthogonal & diagonal) various lengths
        Tile d4 = T('d', 4); Tile h4 = T('h', 4); Tile d8 = T('d', 8); Tile c1 = T('c', 1); Tile g5 = T('g', 5); Tile a4 = T('a', 4); Tile d1 = T('d', 1);
        _queriesEmpty = new() { (rook, d4, h4), (rook, d4, d8), (bishop, c1, g5), (queen, d4, h4), (queen, d4, d8) };

        // Densities: choose a repeatable pseudo random subset for blockers (25% and 50%)
        var rnd = new System.Random(1234);
        var candidateSquares = tiles.Where(t => t != d4 && t != h4 && t != d8 && t != c1 && t != g5).ToList();
        var quarterCount = candidateSquares.Count / 4;
        var halfCount = candidateSquares.Count / 2;
        var quarter = candidateSquares.OrderBy(_ => rnd.Next()).Take(quarterCount).ToHashSet();
        var half = candidateSquares.OrderBy(_ => rnd.Next()).Take(halfCount).ToHashSet();

        _queriesQuarter = _queriesEmpty; // same target set; occupancy changes performance only
        _queriesHalf = _queriesEmpty;

        // Build initial snapshots for each density scenario in OnIterationSetup.
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // For each iteration we rebuild snapshots according to selected Density to avoid incremental mutation costs.
        // Fast-path occupancy not yet benchmarked here (requires GameProgress refactor); placeholder for future extension.
    }

    [Benchmark(Baseline = true)]
    public int LegacyVisitor()
    {
        int count = 0;
        foreach (var q in SelectQueries())
        {
            foreach (var pattern in q.piece.Patterns)
            {
                var visitor = new Veggerby.Boards.Artifacts.Relations.ResolveTilePathPatternVisitor(_game.Board, q.from, q.to);
                pattern.Accept(visitor);
                if (visitor.ResultPath is not null && visitor.ResultPath.To.Equals(q.to)) { count++; break; }
            }
        }
        return count;
    }

    [Benchmark]
    public int Compiled()
    {
        int count = 0;
        foreach (var q in SelectQueries())
        {
            if (_compiled.TryResolve(q.piece, q.from, q.to, out var path) && path is not null) count++;
        }
        return count;
    }

    // Fast-path benchmark deferred until a reusable GameProgress harness is wired for benchmarks.

    private IEnumerable<(Piece piece, Tile from, Tile to)> SelectQueries()
    {
        return Density switch
        {
            "empty" => _queriesEmpty,
            "quarter" => _queriesQuarter,
            "half" => _queriesHalf,
            _ => _queriesEmpty
        };
    }
}