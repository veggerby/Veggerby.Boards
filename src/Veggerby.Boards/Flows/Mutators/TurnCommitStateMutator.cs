using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator handling <see cref="TurnCommitEvent"/> by transitioning the current <see cref="TurnState"/>
/// from Main to End segment (no TurnNumber increment). Inert if sequencing disabled, no turn state present,
/// or current segment is not Main.
/// </summary>
internal sealed class TurnCommitStateMutator : IStateMutator<TurnCommitEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, TurnCommitEvent @event)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return gameState;
        }

        var currentTurn = gameState.GetStates<TurnState>().FirstOrDefault();
        if (currentTurn is null || currentTurn.Segment != TurnSegment.Main)
        {
            return gameState;
        }

        var updatedTurn = new TurnState(currentTurn.Artifact, currentTurn.TurnNumber, TurnSegment.End, currentTurn.PassStreak);
        return gameState.Next([updatedTurn]);
    }
}