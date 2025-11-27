using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that releases a player from jail.
/// </summary>
public class GetOutOfJailStateMutator : IStateMutator<GetOutOfJailGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, GetOutOfJailGameEvent @event)
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

        var newPlayerState = playerState.ReleaseFromJail();

        // Handle method-specific costs
        switch (@event.Method)
        {
            case GetOutOfJailMethod.PaidFine:
                newPlayerState = newPlayerState.AdjustCash(-50);
                break;

            case GetOutOfJailMethod.UsedCard:
                newPlayerState = newPlayerState.WithGetOutOfJailCard(false);
                break;

            case GetOutOfJailMethod.RolledDoubles:
                // No additional cost
                break;
        }

        return gameState.Next([newPlayerState]);
    }
}
