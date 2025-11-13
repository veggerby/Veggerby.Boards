using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Bitboards;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Compares legacy LINQ OrderBy tile ordering vs current insertion sort approach in <see cref="BoardBitboardLayout"/>.
/// Uses synthetic boards of varying sizes up to 64 tiles with randomized identifier orders.
/// </summary>
[MemoryDiagnoser]
public class BitboardLayoutOrderingBenchmark
{
    [Params(8, 16, 32, 48, 64)] public int TileCount { get; set; } = 8;

    private Board _boardRandom = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Build a simple chain board of TileCount tiles with randomized ids to force ordering work.
        var rand = new System.Random(1234 + TileCount);
        var tiles = new List<Tile>(TileCount);
        for (int i = 0; i < TileCount; i++)
        {
            tiles.Add(new Tile($"t{i}-{rand.Next(0, 10_000)}"));
        }
        // Shuffle ids further
        tiles = tiles.OrderBy(_ => rand.Next()).ToList();
        var dir = new Direction("d");
        var rels = new List<TileRelation>(TileCount - 1);
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            rels.Add(new TileRelation(tiles[i], tiles[i + 1], dir));
        }
        // Add wrap to ensure last tile included in relations list referencing it as From or To at least once.
        rels.Add(new TileRelation(tiles[^1], tiles[0], dir));
        _boardRandom = new Board($"bb-chain-{TileCount}", rels);
    }

    [Benchmark(Baseline = true)]
    public object Current_InsertionSort()
    {
        return new BoardBitboardLayout(_boardRandom);
    }

    [Benchmark]
    public object Legacy_LinqOrderBy()
    {
        // Emulate previous implementation using OrderBy.
        var tiles = _boardRandom.Tiles.OrderBy(t => t.Id).ToArray();
        var dict = new Dictionary<Tile, int>(tiles.Length);
        var indexToTile = new Tile[tiles.Length];
        var masks = new Bitboard64[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            dict[tiles[i]] = i;
            indexToTile[i] = tiles[i];
            masks[i] = new Bitboard64(1UL << i);
        }
        return (dict, indexToTile, masks);
    }
}
