using System;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/*
Benchmark Purpose:
Measure append + full enumeration cost of current EventChain persistent structure under varying history sizes.
This establishes baseline before considering alternative persistent vector or chunked list designs.
Scenarios:
1. Incremental single-event append (Append) across N iterations.
2. Batch append (AppendRange) of K events.
3. Enumeration of chain after growth.
Output metrics: Mean append time per event, enumeration throughput (#events/ms), segment count ratio.
*/
/// <summary>
/// Benchmarks growth and enumeration characteristics of the current <see cref="EventChain"/> implementation.
/// Provides baseline metrics (append cost and enumeration throughput) before refactoring toward alternative persistent vectors.
/// </summary>
[MemoryDiagnoser]
public class EventChainGrowthBenchmark
{
    private EventChain _chain = EventChain.Empty;
    private GameProgress _progress = null!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ChessGameBuilder();
        _progress = builder.Compile();
    }

    [Params(100, 1_000, 10_000)]
    public int EventCount;

    [Benchmark(Description = "Append single events")]
    public EventChain AppendSingles()
    {
        var chain = EventChain.Empty;
        for (int i = 0; i < EventCount; i++)
        {
            // Use a no-op event (TurnReplayEvent or NullGameEvent surrogate) to simulate timeline growth
            chain = chain.Append(new NullGameEvent());
        }
        return chain;
    }

    [Benchmark(Description = "AppendRange batches of size 10")]
    public EventChain AppendRangeBatches()
    {
        var chain = EventChain.Empty;
        var batch = new IGameEvent[10];
        for (int i = 0; i < batch.Length; i++)
        {
            batch[i] = new NullGameEvent();
        }
        int remaining = EventCount;
        while (remaining > 0)
        {
            int take = Math.Min(batch.Length, remaining);
            if (take != batch.Length)
            {
                // rebuild partial batch
                for (int i = 0; i < take; i++)
                {
                    batch[i] = new NullGameEvent();
                }
                var slice = new IGameEvent[take];
                Array.Copy(batch, slice, take);
                chain = chain.AppendRange(slice);
            }
            else
            {
                chain = chain.AppendRange(batch);
            }
            remaining -= take;
        }
        return chain;
    }

    [Benchmark(Description = "Enumerate grown chain (Append singles)")]
    public int EnumerateAfterGrowth()
    {
        var chain = EventChain.Empty;
        for (int i = 0; i < EventCount; i++)
        {
            chain = chain.Append(new NullGameEvent());
        }
        int count = 0;
        foreach (var evt in chain)
        {
            if (evt != null)
            {
                count++;
            }
        }
        return count;
    }
}
