namespace Veggerby.Boards.Simulation;

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

/// <summary>
/// Provides parallel playout orchestration with deterministic aggregation (ordering independent when policies are deterministic).
/// </summary>
public static class ParallelSimulator
{
    /// <summary>
    /// Executes multiple playouts in parallel (bounded by <paramref name="degreeOfParallelism"/>).
    /// Determinism: When the supplied policy and stop predicate are deterministic relative to each derived seed, the multiset of terminal state hashes
    /// will be stable across runs (ordering of results list is by playout index for determinism).
    /// </summary>
    public static async Task<PlayoutBatchResult> RunManyAsync(
        GameProgress progress,
        int playoutCount,
        Func<int, PlayoutPolicy> policyFactory,
        PlayoutStopPredicate? stopPredicate = null,
        int maxDepth = 1024,
        int degreeOfParallelism = 0,
        CancellationToken cancellationToken = default)
    {
        if (!FeatureFlags.EnableSimulation)
        {
            throw new InvalidOperationException("Simulation feature disabled – enable FeatureFlags.EnableSimulation to use ParallelSimulator.");
        }

        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(policyFactory);
        if (playoutCount <= 0) throw new ArgumentOutOfRangeException(nameof(playoutCount));
        if (maxDepth <= 0) throw new ArgumentOutOfRangeException(nameof(maxDepth));
        if (degreeOfParallelism < 0) throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));
        if (degreeOfParallelism == 0) degreeOfParallelism = Environment.ProcessorCount;

        var results = new PlayoutResult[playoutCount];

        using var semaphore = new SemaphoreSlim(degreeOfParallelism);
        var tasks = new List<Task>();

        for (int i = 0; i < playoutCount; i++)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var policy = policyFactory(index);
                    var seededProgress = progress; // Future: derive per-index seed via builder clone if RNG needed.
                    var result = SequentialSimulator.RunWithMetrics(seededProgress, policy, stopPredicate, maxDepth);
                    results[index] = result.Result;
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return new PlayoutBatchResult(results);
    }
}

#nullable disable