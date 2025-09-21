using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Outcome of attempting to replay a <see cref="BugReport"/>.
/// </summary>
internal sealed record BugReportReplayResult(
    bool Success,
    string Reason,
    ulong? ExpectedHash64,
    (ulong Low, ulong High)? ExpectedHash128,
    ulong? ActualHash64,
    (ulong Low, ulong High)? ActualHash128,
    int AppliedEvents,
    int ExpectedEvents)
{
    public override string ToString() => Success
        ? $"Replay Success: Events={AppliedEvents} Hash64={ActualHash64} Hash128={(ActualHash128.HasValue ? $"{ActualHash128.Value.Low:X16}{ActualHash128.Value.High:X16}" : "null")}"
        : $"Replay FAILED: {Reason} (Applied {AppliedEvents}/{ExpectedEvents}) ExpectedHash64={ExpectedHash64} ActualHash64={ActualHash64}";
}

/// <summary>
/// Replays a bug report by reconstructing a game using the captured seed and feature flags, then
/// applying a synthesized sequence of events matching recorded type names. Since Phase 1 capture omits
/// payload data, only zero-argument or synthesizable events can be replayed deterministically. For now
/// we only validate the absence of events (empty list) or report inability to synthesize otherwise.
/// </summary>
internal static class BugReportReplayer
{
    /// <summary>
    /// Attempts to replay the specified bug report with the supplied game builder factory.
    /// </summary>
    /// <param name="report">The captured bug report.</param>
    /// <param name="builderFactory">Factory producing a configured <see cref="GameBuilder"/> ready to compile.</param>
    /// <returns>A <see cref="BugReportReplayResult"/> describing success or failure.</returns>
    public static BugReportReplayResult Replay(BugReport report, Func<GameBuilder> builderFactory)
    {
        if (report is null)
        {
            throw new ArgumentNullException(nameof(report));
        }
        if (builderFactory is null)
        {
            throw new ArgumentNullException(nameof(builderFactory));
        }

        // Apply feature flags to mirror original capture context.
        FeatureFlags.EnableDecisionPlan = report.DecisionPlanEnabled;
        FeatureFlags.EnableStateHashing = report.StateHashingEnabled;
        FeatureFlags.EnableTimelineZipper = report.TimelineEnabled;

        var builder = builderFactory();
        // NOTE: Seed application not yet exposed via GameBuilder public API; seeding is implicit in RNG scaffold.
        // Future enhancement: introduce WithSeed/WithRandomSource for deterministic replay.
        var progress = builder.Compile();

        // Early exit if no events captured – validate initial hash only.
        if (report.EventCount == 0)
        {
            var successEmpty = HashesMatch(report, progress.State);
            return new BugReportReplayResult(
                successEmpty,
                successEmpty ? string.Empty : "Initial state hash mismatch",
                report.FinalHash64,
                report.FinalHash128,
                progress.State.Hash,
                progress.State.Hash128,
                0,
                0);
        }

        // Phase 1 limitation: we cannot synthesize events beyond recognizing their types.
        return new BugReportReplayResult(
            false,
            "Event payload synthesis not implemented (Phase 1 BugReport capture omits payload).",
            report.FinalHash64,
            report.FinalHash128,
            progress.State.Hash,
            progress.State.Hash128,
            0,
            report.EventCount);
    }

    private static bool HashesMatch(BugReport report, GameState state)
    {
        if (report.FinalHash64.HasValue && report.FinalHash64.Value != state.Hash)
        {
            return false;
        }
        if (report.FinalHash128.HasValue)
        {
            if (!state.Hash128.HasValue)
            {
                return false;
            }
            if (report.FinalHash128.Value.Low != state.Hash128.Value.Low || report.FinalHash128.Value.High != state.Hash128.Value.High)
            {
                return false;
            }
        }
        return true;
    }
}