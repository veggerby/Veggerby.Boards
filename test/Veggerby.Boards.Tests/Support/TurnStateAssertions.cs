using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Support;

/// <summary>
/// Assertion helpers for verifying turn segment state in tests.
/// </summary>
public static class TurnStateAssertions
{
    /// <summary>
    /// Asserts that the current <see cref="TurnState"/> segment equals the expected value.
    /// </summary>
    /// <param name="progress">Current game progress.</param>
    /// <param name="expected">Expected segment.</param>
    public static void ShouldBeSegment(this GameProgress progress, TurnSegment expected)
    {
        var seg = progress.State.GetStates<TurnState>().First().Segment;
        seg.Should().Be(expected, $"expected segment {expected} before running the next event");
    }

    /// <summary>
    /// Asserts that exactly one TurnState is present and (optionally) sequencing feature flag is enabled.
    /// Provides clearer failure when tests implicitly rely on sequencing being active.
    /// </summary>
    /// <param name="progress">Progress instance.</param>
    /// <param name="requireSequencingEnabled">If true, asserts the feature flag is on.</param>
    public static void ShouldHaveSingleTurnState(this GameProgress progress, bool requireSequencingEnabled = true)
    {
        var states = progress.State.GetStates<TurnState>().ToList();
        states.Count.Should().Be(1, "exactly one TurnState expected for deterministic sequencing tests");
        if (requireSequencingEnabled)
        {
            Veggerby.Boards.Internal.FeatureFlags.EnableTurnSequencing.Should().BeTrue("sequencing must be enabled for deck-building turn segment gating tests");
        }
    }
}