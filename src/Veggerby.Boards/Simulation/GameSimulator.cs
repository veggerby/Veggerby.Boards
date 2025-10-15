using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Simulation;

/// <summary>
/// Provides concurrency-safe simulation utilities for executing deterministic (or stochastic via explicit RNG state) playouts.
/// </summary>
/// <remarks>
/// The simulator never mutates existing progress instances; each playout starts from an immutable baseline
/// and produces a successor chain. Parallel playouts each operate on independent cloned progress graphs.
/// Randomness (dice rolls) must be driven by deterministic <see cref="GameState"/> random sources to achieve reproducibility.
/// </remarks>
/// <remarks>Initializes a new simulator.</remarks>
public sealed class GameSimulator(IPlayoutPolicy policy, PlayoutOptions? options = null)
{
    private readonly IPlayoutPolicy _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    private readonly PlayoutOptions _options = options ?? new PlayoutOptions();

    /// <summary>
    /// Observer hook for diagnostics / instrumentation. Implementations must be thread-safe if re-used across parallel playouts.
    /// </summary>
    public interface IPlayoutObserver
    {
        /// <summary>
        /// Invoked after attempting (and possibly applying) a step. A step corresponds to one policy evaluation cycle.
        /// </summary>
        /// <param name="progress">Progress snapshot BEFORE attempting the step.</param>
        /// <param name="stepIndex">Zero-based index of the step.</param>
        /// <param name="candidateCount">Number of candidate events produced this step.</param>
        /// <param name="applied">True if an event applied.</param>
        /// <param name="attempted">The event that was attempted (and applied) when <paramref name="applied"/> is true; otherwise the last attempted candidate or null when none.</param>
    void OnStep(GameProgress progress, int stepIndex, int candidateCount, bool applied, IGameEvent? attempted);

        /// <summary>
        /// Invoked once a playout completes (terminal state reached or safety cap triggered).
        /// </summary>
        /// <param name="result">Result summary.</param>
        void OnCompleted(PlayoutResult result);
    }

    private sealed class NullObserver : IPlayoutObserver
    {
        public static readonly NullObserver Instance = new();
        private NullObserver() { }
    public void OnStep(GameProgress progress, int stepIndex, int candidateCount, bool applied, IGameEvent? attempted) { }
        public void OnCompleted(PlayoutResult result) { }
    }

    /// <summary>
    /// Policy wrapper that randomizes candidate ordering using the state's deterministic RNG source.
    /// </summary>
    private sealed class RandomChoicePlayoutPolicy(IPlayoutPolicy inner) : IPlayoutPolicy
    {
        private readonly IPlayoutPolicy _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        public IEnumerable<IGameEvent> GetCandidateEvents(GameProgress progress)
        {
            var candidates = _inner.GetCandidateEvents(progress);
            if (candidates is null)
            {
                return Enumerable.Empty<IGameEvent>();
            }

            // Materialize to array for shuffle – policy is expected to produce modest counts; if large, consider streaming approach.
            var arr = candidates as IGameEvent[] ?? candidates.ToArray();
            var random = progress.State.Random; // deterministic per state snapshot
            if (arr.Length <= 1 || random is null)
            {
                return arr;
            }

            // Fisher-Yates using deterministic random source (NextInt32 assumed stable contract if implemented; fallback to modulo on NextUInt32 if needed)
            for (var i = arr.Length - 1; i > 0; i--)
            {
                // Use unsigned path to avoid negative modulo bias.
                var r = (int)(random.NextUInt() % (uint)(i + 1));
                (arr[i], arr[r]) = (arr[r], arr[i]);
            }

            return arr;
        }
    }

    /// <summary>
    /// Composite policy: iterates provided policies in order and returns the first non-empty candidate sequence.
    /// </summary>
    private sealed class CompositePlayoutPolicy : IPlayoutPolicy
    {
        private readonly IPlayoutPolicy[] _policies;
        public CompositePlayoutPolicy(IEnumerable<IPlayoutPolicy> policies)
        {
            _policies = policies?.ToArray() ?? throw new ArgumentNullException(nameof(policies));
            if (_policies.Length == 0)
            {
                throw new ArgumentException("At least one policy required", nameof(policies));
            }
        }

        public IEnumerable<IGameEvent> GetCandidateEvents(GameProgress progress)
        {
            foreach (var p in _policies)
            {
                var seq = p.GetCandidateEvents(progress) ?? Enumerable.Empty<IGameEvent>();
                // materialize minimal look-ahead to determine emptiness without double enumeration
                if (seq is IGameEvent[] arr)
                {
                    if (arr.Length > 0)
                    {
                        return arr;
                    }
                }
                else
                {
                    using var e = seq.GetEnumerator();
                    if (e.MoveNext())
                    {
                        // build list including first
                        var list = new List<IGameEvent> { e.Current };
                        while (e.MoveNext())
                        {
                            list.Add(e.Current);
                        }
                        return list;
                    }
                }
            }
            return Enumerable.Empty<IGameEvent>();
        }
    }

    /// <summary>
    /// Wraps the current simulator with a randomized candidate ordering policy (deterministic per GameState RNG).
    /// </summary>
    public GameSimulator WithRandomizedPolicy() => new(new RandomChoicePlayoutPolicy(_policy), _options);

    /// <summary>
    /// Wraps the current simulator with a composite policy that evaluates policies in order and selects the first non-empty candidate set.
    /// </summary>
    public GameSimulator WithCompositePolicy(params IPlayoutPolicy[] policies)
        => new(new CompositePlayoutPolicy(new[] { _policy }.Concat(policies ?? Array.Empty<IPlayoutPolicy>())), _options);

    /// <summary>
    /// Executes a single playout synchronously.
    /// </summary>
    /// <param name="progress">Initial progress snapshot.</param>
    /// <param name="observer">Optional diagnostics observer (receives per-step and completion callbacks).</param>
    /// <returns>Playout result.</returns>
    public PlayoutResult Playout(GameProgress progress, IPlayoutObserver? observer = null)
    {
        ArgumentNullException.ThrowIfNull(progress);

        var ob = observer ?? NullObserver.Instance;

    List<GameState>? trace = _options.CaptureTrace ? new List<GameState>() : null;
        if (trace is not null)
        {
            trace.Add(progress.State);
        }

        var applied = 0;
        var current = progress;
        var started = DateTime.UtcNow;
        var stepIndex = 0;

        while (true)
        {
            if (_options.ShouldStop(applied, started))
            {
                var terminal = new PlayoutResult(progress, current, applied, _options.TimeLimit.HasValue ? PlayoutTerminalReason.TimeLimit : PlayoutTerminalReason.MaxDepth, trace);
                ob.OnCompleted(terminal);
                return terminal;
            }

            var candidates = _policy.GetCandidateEvents(current) ?? Enumerable.Empty<IGameEvent>();
            var any = false;
            int candidateCount = 0;
            IGameEvent? attempted = null;

            foreach (var evt in candidates)
            {
                any = true;
                candidateCount++;
                attempted = evt;
                var result = current.HandleEventResult(evt);

                if (result.Applied)
                {
                    applied++;
                    current = new GameProgress(current.Engine, result.State, current.Events.Concat([evt]));

                    if (trace is not null)
                    {
                        trace.Add(current.State);
                    }

                    ob.OnStep(progress: current, stepIndex: stepIndex, candidateCount: candidateCount, applied: true, attempted: attempted);
                    stepIndex++;

                    break; // re-evaluate candidates after each successful application
                }
            }

            if (!any)
            {
                var terminal = new PlayoutResult(progress, current, applied, PlayoutTerminalReason.NoMoves, trace);
                ob.OnCompleted(terminal);
                return terminal;
            }

            if (any && attempted is not null && (candidateCount > 0))
            {
                // We only call OnStep when an event applied above. For completeness, if none applied but candidates existed, invoke observer.
                // (Edge case: all candidates rejected/ignored.)
                // Determine if state progressed this step: applied already handled; here applied=false.
                // Only reach here if loop ended due to break? Actually if break triggered we already notified; otherwise we iterated all candidates without apply.
                // In that case candidateCount>0 and applied event not found.
                // Provide snapshot (current unchanged) and mark applied=false.
                // Note: current already points to last state (unchanged) since no application.
                var appliedThisStep = current != progress && applied > stepIndex; // but simpler: false.
                if (appliedThisStep == false && applied <= stepIndex)
                {
                    ob.OnStep(progress: current, stepIndex: stepIndex, candidateCount: candidateCount, applied: false, attempted: attempted);
                    stepIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Executes multiple playouts in parallel.
    /// </summary>
    /// <param name="progress">Baseline progress snapshot.</param>
    /// <param name="count">Number of independent playouts.</param>
    /// <param name="degreeOfParallelism">Optional max parallel workers (default: Environment.ProcessorCount).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="simulatorFactory">Optional factory to create a derived simulator per worker (e.g., wrapping policy with thread-local state).</param>
    /// <returns>Aggregated batch result.</returns>
    public async Task<PlayoutBatchResult> PlayoutManyAsync(GameProgress progress, int count, int? degreeOfParallelism = null, CancellationToken cancellationToken = default, Func<GameSimulator>? simulatorFactory = null)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var dop = degreeOfParallelism.GetValueOrDefault(Environment.ProcessorCount);
        var results = new ConcurrentBag<PlayoutResult>();
        using var semaphore = new SemaphoreSlim(dop);
        var tasks = new List<Task>();

        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var cloned = new GameProgress(progress.Engine, progress.State, progress.Events); // engine/state immutable so reuse references
                    var sim = simulatorFactory?.Invoke() ?? this;
                    var r = sim.Playout(cloned);
                    results.Add(r);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return new PlayoutBatchResult(results.ToList());
    }

    /// <summary>
    /// Executes sequential playouts until either <paramref name="maxCount"/> is reached or <paramref name="stopPredicate"/> returns true.
    /// </summary>
    /// <param name="progress">Baseline progress.</param>
    /// <param name="maxCount">Maximum number of playouts to execute (must be &gt; 0).</param>
    /// <param name="stopPredicate">Convergence / stopping predicate evaluated after each playout with current batch snapshot.</param>
    /// <returns>Batch result at termination.</returns>
    public PlayoutBatchResult PlayoutManyUntil(GameProgress progress, int maxCount, Func<PlayoutBatchResult, bool> stopPredicate)
    {
        ArgumentNullException.ThrowIfNull(progress);
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount));
        }
        ArgumentNullException.ThrowIfNull(stopPredicate);

        var list = new List<PlayoutResult>(capacity: Math.Min(maxCount, 1024));
        for (var i = 0; i < maxCount; i++)
        {
            var cloned = new GameProgress(progress.Engine, progress.State, progress.Events);
            var r = Playout(cloned);
            list.Add(r);
            var snapshot = new PlayoutBatchResult(list.ToList()); // create fresh snapshot so metrics reflect current accumulation
            if (stopPredicate(snapshot))
            {
                return snapshot;
            }
        }

        return new PlayoutBatchResult(list);
    }

    /// <summary>
    /// Executes playouts in parallel waves, evaluating <paramref name="stopPredicate"/> between waves for early termination.
    /// </summary>
    /// <param name="progress">Baseline progress.</param>
    /// <param name="maxCount">Upper bound on total playouts.</param>
    /// <param name="waveSize">Number of playouts per parallel wave (defaults to logical processor count).</param>
    /// <param name="stopPredicate">Stopping predicate evaluated after each wave with cumulative batch snapshot.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Batch result at termination.</returns>
    public async Task<PlayoutBatchResult> PlayoutManyUntilAsync(GameProgress progress, int maxCount, Func<PlayoutBatchResult, bool> stopPredicate, int? waveSize = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(progress);
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount));
        }
        ArgumentNullException.ThrowIfNull(stopPredicate);

        var list = new List<PlayoutResult>(capacity: Math.Min(maxCount, 1024));
        var remaining = maxCount;
        var size = waveSize.GetValueOrDefault(Environment.ProcessorCount);

        while (remaining > 0)
        {
            var take = Math.Min(size, remaining);
            var wave = await PlayoutManyAsync(progress, take, degreeOfParallelism: size, cancellationToken: cancellationToken).ConfigureAwait(false);
            list.AddRange(wave.Results);
            var snapshot = new PlayoutBatchResult(list.ToList());
            if (stopPredicate(snapshot))
            {
                return snapshot;
            }
            remaining -= take;
        }

        return new PlayoutBatchResult(list);
    }
}