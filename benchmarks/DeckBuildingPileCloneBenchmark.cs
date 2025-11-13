using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Microbenchmark harness establishing a baseline for deck pile cloning cost during typical mutation scenarios.
/// </summary>
/// <remarks>
/// The current deck building implementation performs dictionary cloning of piles on each mutation. This benchmark will
/// help quantify allocation and time cost for varying pile counts and mutation frequencies prior to any refactor
/// introducing copy-on-write or structural sharing optimizations.
/// </remarks>
public class DeckBuildingPileCloneBenchmark
{
    [Params(8, 32, 128)]
    public int PileCount
    {
        get; set;
    }

    [Params(1, 4, 16)]
    public int MutationsPerIteration
    {
        get; set;
    }

    private GameProgress _progress = null!; // initialized in GlobalSetup
    private List<string> _pileIds = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // TODO: Construct a representative deck-building game state with PileCount piles.
        // Placeholder: will be implemented when deck-building game builder utilities are profiled.
        _pileIds = new List<string>(PileCount);
        for (var i = 0; i < PileCount; i++)
        {
            _pileIds.Add($"pile-{i}");
        }

        // _progress = ... build deck game progress
    }

    [Benchmark(Description = "Clone piles on mutations")]
    public int ClonePilesOnMutations()
    {
        if (_progress is null)
        {
            return 0; // placeholder early exit until implemented
        }

        var hash = 0;
        // Simulate MutationsPerIteration pile mutations (placeholder logic).
        for (var m = 0; m < MutationsPerIteration; m++)
        {
            var id = _pileIds[m % _pileIds.Count];
            // TODO: Perform a representative pile mutation triggering cloning; accumulate hash of resulting state artifact to avoid dead-code elimination.
            hash ^= id.GetHashCode();
        }

        return hash;
    }
}
