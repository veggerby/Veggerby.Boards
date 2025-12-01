using System;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles a player staying in jail for another turn.
/// </summary>
/// <remarks>
/// This mutator increments the jail turn counter for the player.
/// When a player has been in jail for 3 turns, they must pay $50
/// to get out (handled via <see cref="GetOutOfJailGameEvent"/>).
/// </remarks>
public class StayInJailStateMutator : IStateMutator<StayInJailGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, StayInJailGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var playerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            throw new InvalidOperationException($"Player state not found for {@event.Player.Id}");
        }

        if (!playerState.InJail)
        {
            throw new InvalidOperationException($"Player {@event.Player.Id} is not in jail");
        }

        var newPlayerState = playerState.IncrementJailTurns();

        return gameState.Next([newPlayerState]);
    }
}
