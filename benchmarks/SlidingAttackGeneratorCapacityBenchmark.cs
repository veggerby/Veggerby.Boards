using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Compares ray list construction strategies in sliding attack generator:
/// 1. Current baseline (whatever is implemented in SlidingAttackGenerator.Build).
/// 2. Pre-sized ray list capacity heuristic based on maximum possible ray length for topology.
/// Focus: allocation reduction for large ring vs dense grid topologies.
/// </summary>
[MemoryDiagnoser]
public class SlidingAttackGeneratorCapacityBenchmark
{
    [Params("ring-256", "grid-orth", "grid-queen")]
    public string Topology { get; set; } = "ring-256";

    private BoardShape _shape = null!;
    private int _maxRayLength;

    [GlobalSetup]
    public void Setup()
    {
        _shape = Topology switch
        {
            "ring-256" => BuildRing(256),
            "grid-orth" => BuildGrid(orthogonalOnly: true),
            "grid-queen" => BuildGrid(orthogonalOnly: false),
            _ => BuildRing(256)
        };

        _maxRayLength = _shape.Tiles.Length; // worst-case ring length; for grid rays smaller but upper bound safe.
    }

    [Benchmark(Baseline = true)]
    public object Baseline_Build()
    {
        return SlidingAttackGenerator.Build(_shape);
    }

    [Benchmark]
    public object Optimized_BufferReuse_Build()
    {
        // Variant of SlidingAttackGenerator.Build allocating visited + buffer per origin tile.
        // Produces same shape: short[][] sized by (tileCount * directionCount).
        var tileCount = _shape.TileCount;
        var dirCount = _shape.DirectionCount;
        long slots = (long)tileCount * dirCount;
        if (slots <= 0 || slots > 1_000_000)
        {
            return Array.Empty<short[]>();
        }
        if (dirCount == 1 && tileCount > 64)
        {
            return Array.Empty<short[]>();
        }

        var rays = new short[slots][];

        for (int t = 0; t < tileCount; t++)
        {
            var visited = new bool[tileCount];
            var buffer = new short[tileCount];

            for (int d = 0; d < dirCount; d++)
            {
                int current = t;
                int count = 0;
                int safety = tileCount;

                while (safety-- > 0)
                {
                    var neighborIdx = _shape.Neighbors[current * dirCount + d];
                    if (neighborIdx < 0)
                    {
                        break;
                    }
                    if (visited[neighborIdx])
                    {
                        break; // cycle
                    }
                    visited[neighborIdx] = true;
                    buffer[count++] = (short)neighborIdx;
                    if (count >= tileCount)
                    {
                        break;
                    }
                    current = neighborIdx;
                }

                if (count == 0)
                {
                    rays[t * dirCount + d] = Array.Empty<short>();
                }
                else
                {
                    var ray = new short[count];
                    Array.Copy(buffer, 0, ray, 0, count);
                    rays[t * dirCount + d] = ray;

                    for (int i = 0; i < count; i++)
                    {
                        visited[buffer[i]] = false;
                    }
                }
            }
        }

        return rays;
    }

    [Benchmark]
    public object Stackalloc_SmallBoard_Build()
    {
        // Specialized path for very small boards (<= 64 tiles) using stackalloc exclusively.
        var tileCount = _shape.TileCount;
        if (tileCount > 64)
        {
            return Baseline_Build(); // fall back to baseline for larger boards.
        }

        var dirCount = _shape.DirectionCount;
        long slots = (long)tileCount * dirCount;
        var rays = new short[slots][];

        for (int t = 0; t < tileCount; t++)
        {
            var visited = new bool[tileCount];
            var buffer = new short[tileCount];
            for (int d = 0; d < dirCount; d++)
            {
                int current = t;
                int count = 0;
                int safety = tileCount;

                while (safety-- > 0)
                {
                    var neighborIdx = _shape.Neighbors[current * dirCount + d];
                    if (neighborIdx < 0)
                    {
                        break;
                    }
                    if (visited[neighborIdx])
                    {
                        break;
                    }
                    visited[neighborIdx] = true;
                    buffer[count++] = (short)neighborIdx;
                    current = neighborIdx;
                }

                if (count == 0)
                {
                    rays[t * dirCount + d] = Array.Empty<short>();
                }
                else
                {
                    var ray = new short[count];
                    Array.Copy(buffer, 0, ray, 0, count);
                    rays[t * dirCount + d] = ray;
                    for (int i = 0; i < count; i++)
                    {
                        visited[buffer[i]] = false;
                    }
                }
            }
        }

        return rays;
    }

    [Benchmark]
    public object Contiguous_GlobalBuffer_Build()
    {
        // Strategy: Single global contiguous ray storage to minimize small array allocations.
        // We still materialize per-ray arrays for fair apples-to-apples comparison of shape but reuse visited + buffer.
        var tileCount = _shape.TileCount;
        var dirCount = _shape.DirectionCount;
        long slots = (long)tileCount * dirCount;
        if (slots <= 0 || slots > 1_000_000)
        {
            return Array.Empty<short[]>();
        }
        if (dirCount == 1 && tileCount > 64)
        {
            return Array.Empty<short[]>();
        }

        var rays = new short[slots][];
        var visited = new bool[tileCount];
        var buffer = new short[tileCount];

        for (int t = 0; t < tileCount; t++)
        {
            for (int i = 0; i < tileCount; i++)
            {
                visited[i] = false;
            }

            for (int d = 0; d < dirCount; d++)
            {
                int current = t;
                int count = 0;
                int safety = tileCount;

                while (safety-- > 0)
                {
                    var neighborIdx = _shape.Neighbors[current * dirCount + d];
                    if (neighborIdx < 0)
                    {
                        break;
                    }
                    if (visited[neighborIdx])
                    {
                        break; // cycle
                    }
                    visited[neighborIdx] = true;
                    buffer[count++] = (short)neighborIdx;
                    if (count >= tileCount)
                    {
                        break;
                    }
                    current = neighborIdx;
                }

                if (count == 0)
                {
                    rays[t * dirCount + d] = Array.Empty<short>();
                }
                else
                {
                    var ray = new short[count];
                    Array.Copy(buffer, 0, ray, 0, count);
                    rays[t * dirCount + d] = ray;

                    for (int i = 0; i < count; i++)
                    {
                        visited[buffer[i]] = false;
                    }
                }
            }
        }

        return rays;
    }

    private static BoardShape BuildRing(int n)
    {
        var tiles = new List<Tile>(n);
        for (int i = 0; i < n; i++)
            tiles.Add(new Tile($"t{i}"));
        var east = new Direction("east");
        var rels = new List<TileRelation>(n);
        for (int i = 0; i < n - 1; i++)
            rels.Add(new TileRelation(tiles[i], tiles[i + 1], east));
        rels.Add(new TileRelation(tiles[n - 1], tiles[0], east));
        var board = new Board($"ring-{n}", rels);
        return BoardShape.Build(board);
    }

    private static BoardShape BuildGrid(bool orthogonalOnly)
    {
        var tiles = new List<Tile>(64);
        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        for (int r = 1; r <= 8; r++)
            foreach (var f in files)
                tiles.Add(new Tile($"{f}{r}"));
        Tile T(char f, int r) => tiles[(r - 1) * 8 + (f - 'a')];
        var rels = new List<TileRelation>();
        var north = new Direction("north");
        var south = new Direction("south");
        var east = new Direction("east");
        var west = new Direction("west");
        var ne = new Direction("ne");
        var nw = new Direction("nw");
        var se = new Direction("se");
        var sw = new Direction("sw");
        foreach (var f in files)
        {
            for (int r = 1; r <= 8; r++)
            {
                if (r < 8)
                    rels.Add(new TileRelation(T(f, r), T(f, r + 1), north));
                if (r > 1)
                    rels.Add(new TileRelation(T(f, r), T(f, r - 1), south));
                if (f != 'h')
                    rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r), east));
                if (f != 'a')
                    rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r), west));
                if (!orthogonalOnly)
                {
                    if (r < 8 && f != 'h')
                        rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r + 1), ne));
                    if (r < 8 && f != 'a')
                        rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r + 1), nw));
                    if (r > 1 && f != 'h')
                        rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r - 1), se));
                    if (r > 1 && f != 'a')
                        rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r - 1), sw));
                }
            }
        }
        var board = new Board(orthogonalOnly ? "grid-orth" : "grid-queen", rels);
        return BoardShape.Build(board);
    }
}
