using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator handling <see cref="TurnCommitEvent"/> by transitioning the current <see cref="TurnState"/>
/// from Main to End segment (no TurnNumber increment). Inert if sequencing disabled, no turn state present,
/// or current segment is not Main.
/// </summary>
/// <summary>
/// State mutator transitioning Main segment to End without advancing numeric turn.
/// </summary>
internal sealed class TurnCommitStateMutator : IStateMutator<TurnCommitEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, TurnCommitEvent @event)
    {
        TurnState? currentTurn = null;
        foreach (var ts in gameState.GetStates<TurnState>())
        {
            currentTurn = ts;
            break;
        }
        if (currentTurn is null || currentTurn.Segment != TurnSegment.Main)
        {
            return gameState;
        }

        var updatedTurn = new TurnState(currentTurn.Artifact, currentTurn.TurnNumber, TurnSegment.End, currentTurn.PassStreak);
        return gameState.Next([updatedTurn]);
    }
}