using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Advances the global turn timeline: increments the numeric turn and resets the segment to <see cref="TurnSegment.Start"/>.
/// </summary>
/// <remarks>
/// Rotation of active player (if any) will be added in a future phase once player turn order semantics are formalized.
/// This mutator is only active when <c>FeatureFlags.EnableTurnSequencing</c> is true; callers must gate invocation via rules.
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
        var currentTurnState = gameState.GetStates<TurnState>().FirstOrDefault();
        if (currentTurnState is null)
        {
            return gameState; // no turn state present
        }
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
            var progressed = new TurnState(turnArtifact, currentTurnState.TurnNumber, nextSegment);
            return gameState.Next([progressed]);
        }

        // Last segment: advance numeric turn and reset to Start
        var newState = new TurnState(turnArtifact, currentTurnState.TurnNumber + 1, TurnSegment.Start);
        return gameState.Next([newState]);
    }
}