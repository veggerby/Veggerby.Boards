using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles buying a house on a property.
/// </summary>
public class BuyHouseStateMutator : IStateMutator<BuyHouseGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, BuyHouseGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Get current ownership state
        var ownership = gameState.GetExtras<PropertyOwnershipState>()
            ?? new PropertyOwnershipState();

        // Add house to property
        var newOwnership = ownership.AddHouse(@event.PropertyPosition);

        // Get player state and deduct house cost
        var playerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            throw new InvalidOperationException($"Player state not found for {@event.Player.Id}");
        }

        var newPlayerState = playerState.AdjustCash(-squareInfo.HouseCost);

        // Update ownership via ReplaceExtras and player state via Next
        var stateWithOwnership = gameState.ReplaceExtras(newOwnership);

        return stateWithOwnership.Next([newPlayerState]);
    }
}
