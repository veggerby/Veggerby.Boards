using System;

using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator handling <see cref="CommitActionEvent"/> by recording the player's committed action
/// in the <see cref="StagedEventsState"/>.
/// </summary>
/// <remarks>
/// If no <see cref="StagedEventsState"/> exists, this mutator is inert (no-op). Otherwise, it
/// records the commitment and removes the player from the pending set. Throws if the player
/// has already committed or is not expected to commit.
/// </remarks>
internal sealed class CommitActionStateMutator : IStateMutator<CommitActionEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, CommitActionEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        StagedEventsState? currentStaged = null;
        foreach (var state in gameState.GetStates<StagedEventsState>())
        {
            currentStaged = state;
            break;
        }

        if (currentStaged is null)
        {
            return gameState;
        }

        var updatedStaged = currentStaged.AddCommitment(@event.Player, @event.Action);
        return gameState.Next([updatedStaged]);
    }
}
