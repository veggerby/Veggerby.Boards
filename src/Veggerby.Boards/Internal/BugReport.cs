using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Immutable capture of a deterministic execution slice for later replay or analysis.
/// </summary>
/// <remarks>
/// Capture focuses on minimal reproducibility data: seed, feature flags, event types, final state hashes.
/// Event payloads are not serialized yet (Phase 1) – replay will require deterministic regeneration or
/// later extended schema. This scaffold marks initial <c>BugReport</c> work as partial.
/// </remarks>
internal sealed record BugReport(
    DateTimeOffset CapturedAtUtc,
    ulong Seed,
    bool DecisionPlanEnabled,
    bool StateHashingEnabled,
    bool TimelineEnabled,
    IReadOnlyList<string> EventTypeNames,
    ulong? FinalHash64,
    (ulong Low, ulong High)? FinalHash128,
    int EventCount)
{
    /// <summary>
    /// Friendly representation (for logging or debugging only).
    /// </summary>
    public override string ToString()
    {
        return $"BugReport[Events={EventCount}, Seed={Seed}, Hash64={FinalHash64}, Hash128={(FinalHash128.HasValue ? $"{FinalHash128.Value.Low:X16}{FinalHash128.Value.High:X16}" : "null")}]";
    }

    /// <summary>
    /// Creates a bug report from the supplied progress snapshot.
    /// </summary>
    public static BugReport Capture(GameProgress progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }
        var seed = progress.State.Random?.Seed ?? 0UL;
        var events = progress.Events?.ToList() ?? new List<IGameEvent>();
        return new BugReport(
            DateTimeOffset.UtcNow,
            seed,
            FeatureFlags.EnableDecisionPlan,
            FeatureFlags.EnableStateHashing,
            FeatureFlags.EnableTimelineZipper,
            events.Select(e => e.GetType().FullName).ToArray(),
            progress.State.Hash,
            progress.State.Hash128,
            events.Count);
    }
}