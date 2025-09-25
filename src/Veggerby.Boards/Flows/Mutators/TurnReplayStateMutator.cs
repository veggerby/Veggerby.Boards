using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Applies an extra turn: increments TurnNumber, resets segment to Start, preserves ActivePlayer, resets pass streak.
/// </summary>
public sealed class TurnReplayStateMutator : IStateMutator<TurnReplayEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, TurnReplayEvent gameEvent)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return state; // inert when sequencing disabled
        }

        // Locate existing TurnState; do nothing if absent (shadow emission may be disabled)
        var currentTurn = state.GetStates<TurnState>().FirstOrDefault();
        if (currentTurn is null)
        {
            return state;
        }

        // Extra turn semantics: increment numeric turn, reset segment to Start, reset pass streak, DO NOT rotate active player
        var replayed = new TurnState(currentTurn.Artifact, currentTurn.TurnNumber + 1, TurnSegment.Start, 0);
        return state.Next([replayed]);
    }
}