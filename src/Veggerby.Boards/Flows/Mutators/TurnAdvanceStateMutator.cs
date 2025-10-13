using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Advances the global turn timeline. For non-terminal segments progresses to next segment; for the final segment
/// increments numeric turn, resets to <see cref="TurnSegment.Start"/>, and rotates active player (if projections present).
/// </summary>
/// <remarks>
/// Rotation logic is centralized in <see cref="TurnSequencingHelpers"/>. Executed only when sequencing enabled.
/// Allocation conscious: simple loops, no LINQ in hot path.
/// </remarks>
internal sealed class TurnAdvanceStateMutator : IStateMutator<EndTurnSegmentEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EndTurnSegmentEvent @event)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return gameState; // inert when sequencing disabled
        }

        // Locate existing TurnState (shadow mode injects exactly one)
        TurnState currentTurnState = null;
        foreach (var ts in gameState.GetStates<TurnState>())
        {
            currentTurnState = ts; // only one expected; take first
            break;
        }
        if (currentTurnState is null) { return gameState; }

        var turnArtifact = currentTurnState.Artifact;
        if (currentTurnState.Segment != @event.Segment)
        {
            return gameState; // segment mismatch: ignore (rule should prevent this path)
        }

        var profile = TurnProfile.Default; // future: injectable
        if (!profile.IsLast(@event.Segment))
        {
            // Not last segment: begin next segment instead of full advancement (will be separate mutator in future)
            var nextSegment = profile.Next(@event.Segment)!.Value;
            var progressed = new TurnState(turnArtifact, currentTurnState.TurnNumber, nextSegment, currentTurnState.PassStreak);
            return gameState.Next([progressed]);
        }

        // Last segment: advance numeric turn and reset to Start + rotate via helper
        var advancedTurnState = new TurnState(turnArtifact, currentTurnState.TurnNumber + 1, TurnSegment.Start, 0);
        return TurnSequencingHelpers.ApplyTurnAndRotate(engine, gameState, advancedTurnState);
    }
}