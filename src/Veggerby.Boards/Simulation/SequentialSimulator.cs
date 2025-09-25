namespace Veggerby.Boards.Simulation;

#nullable enable

using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

/// <summary>
/// Provides a deterministic sequential playout engine producing a terminal <see cref="GameProgress"/> given
/// a starting progress instance, a playout policy, and a stop predicate.
/// </summary>
/// <remarks>
/// Determinism: Given the same initial state (including RNG seed if used by policy), identical feature flag configuration,
/// and a pure deterministic <see cref="PlayoutPolicy"/> / stop predicate, this simulator will always produce the same
/// resulting terminal state.
/// The simulator itself performs no randomness; any stochastic behavior must be encoded in the provided policy (which must
/// use engine RNG state snapshots for determinism).
/// Style Charter: Explicit braces, no LINQ in the loop, immutable state transitions (never mutate <see cref="GameState"/>).
/// </remarks>
public static class SequentialSimulator
{
    /// <summary>
    /// Executes a playout until the policy yields <c>null</c> or the stop predicate returns <c>true</c>.
    /// </summary>
    /// <param name="progress">Starting game progress (provides engine + current state).</param>
    /// <param name="policy">Deterministic event selection policy.</param>
    /// <param name="stop">Stop predicate; if <c>null</c>, a default predicate that never stops early is used.</param>
    /// <param name="maxDepth">Optional hard safety cap on number of events to apply (guards runaway policies). Default 1024.</param>
    /// <returns>Terminal <see cref="GameProgress"/> after playout completes.</returns>
    /// <exception cref="InvalidOperationException">Thrown if simulation feature flag is disabled.</exception>
    /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
    public static GameProgress Run(GameProgress progress, PlayoutPolicy policy, PlayoutStopPredicate? stop = null, int maxDepth = 1024)
    {
        return RunWithMetrics(progress, policy, stop, maxDepth).TerminalProgress;
    }

    /// <summary>
    /// Executes a detailed playout returning both the structured <see cref="PlayoutResult"/> and a <see cref="PlayoutMetrics"/> snapshot.
    /// </summary>
    /// <remarks>
    /// This method is allocation conscious: only a single metrics instance and the result record are created (plus empty trace array).
    /// </remarks>
    public static PlayoutDetailedResult RunDetailed(GameProgress progress, PlayoutPolicy policy, PlayoutStopPredicate? stop = null, int maxDepth = 1024)
    {
        var with = RunWithMetrics(progress, policy, stop, maxDepth);
        var r = with.Result;
        var metrics = new PlayoutMetrics(r.AppliedEvents, 0, r.AppliedEvents + 1, r.AppliedEvents); // Placeholder mapping until rejection counting exposed publicly
        return new PlayoutDetailedResult(with.Result, metrics, with.TerminalProgress);
    }

    /// <summary>
    /// Executes a playout producing metrics and a structured result.
    /// </summary>
    public static PlayoutResultWithProgress RunWithMetrics(GameProgress progress, PlayoutPolicy policy, PlayoutStopPredicate? stop = null, int maxDepth = 1024)
    {
        if (!FeatureFlags.EnableSimulation)
        {
            throw new InvalidOperationException("Simulation feature disabled – enable FeatureFlags.EnableSimulation to use SequentialSimulator.");
        }

        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(policy);
        if (maxDepth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth));
        }

        var stopPredicate = stop ?? ((_, _, _) => false);
        var initialState = progress.State;
        var current = progress;
        var depth = 0;
        var applied = 0;
        var rejected = 0;
        var policyCalls = 0;
        PlayoutTerminalReason reason = PlayoutTerminalReason.None;

        while (depth < maxDepth)
        {
            if (stopPredicate(initialState, current.State, depth))
            {
                reason = PlayoutTerminalReason.StopPredicate;
                break;
            }

            policyCalls++;
            var nextEvent = policy(current.State);
            if (nextEvent is null)
            {
                reason = depth == 0 ? PlayoutTerminalReason.NoMoves : PlayoutTerminalReason.PolicyReturnedNull;
                break;
            }

            var next = current.HandleEvent(nextEvent);
            if (ReferenceEquals(next.State, current.State))
            {
                rejected++;
                reason = PlayoutTerminalReason.PolicyReturnedNull; // treat as terminal no progress
                break;
            }

            applied++;
            current = next;
            depth++;
        }

        if (reason == PlayoutTerminalReason.None)
        {
            reason = depth >= maxDepth ? PlayoutTerminalReason.MaxDepth : PlayoutTerminalReason.PolicyReturnedNull;
        }

        // Map metrics into existing record model: Trace not captured for new simulator (empty list)
        var trace = Array.Empty<GameState>();
        var result = new PlayoutResult(progress, current, applied, reason, trace);
        return new PlayoutResultWithProgress(result, current);
    }
}

/// <summary>
/// Convenience wrapper pairing a structured playout result with the terminal progress chain.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PlayoutResultWithProgress"/> class.
/// </remarks>
public sealed class PlayoutResultWithProgress(PlayoutResult result, GameProgress terminalProgress)
{
    /// <summary>
    /// Gets the structured playout result (metrics + terminal reason + depth).
    /// </summary>
    public PlayoutResult Result { get; } = result;

    /// <summary>
    /// Gets the terminal <see cref="GameProgress"/> produced by the playout.
    /// </summary>
    public GameProgress TerminalProgress { get; } = terminalProgress;
}

/// <summary>
/// Detailed playout result containing the basic structured result plus explicit metrics and terminal progress.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PlayoutDetailedResult"/> class.
/// </remarks>
public sealed class PlayoutDetailedResult(PlayoutResult result, PlayoutMetrics metrics, GameProgress terminalProgress)
{
    /// <summary>
    /// Gets the structured playout result (applied events + terminal reason).
    /// </summary>
    public PlayoutResult Result { get; } = result;

    /// <summary>
    /// Gets the metrics captured for this playout.
    /// </summary>
    public PlayoutMetrics Metrics { get; } = metrics;

    /// <summary>
    /// Gets the terminal progress (game + final state chain root) after the playout.
    /// </summary>
    public GameProgress TerminalProgress { get; } = terminalProgress;
}

#nullable disable