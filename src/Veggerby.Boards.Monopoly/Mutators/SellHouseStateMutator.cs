using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles selling a house from a property.
/// </summary>
public class SellHouseStateMutator : IStateMutator<SellHouseGameEvent>
{
    /// <summary>
    /// The sell price multiplier (houses sell for half their purchase price).
    /// </summary>
    public const decimal SellPriceMultiplier = 0.5m;

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, SellHouseGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Get current ownership state
        var ownership = gameState.GetExtras<PropertyOwnershipState>()
            ?? new PropertyOwnershipState();

        // Remove house from property
        var currentHouses = ownership.GetHouseCount(@event.PropertyPosition);
        var newOwnership = ownership.SetHouseCount(@event.PropertyPosition, currentHouses - 1);

        // Get player state and add house sale value (half of purchase price)
        var playerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            throw new InvalidOperationException($"Player state not found for {@event.Player.Id}");
        }

        var saleValue = (int)(squareInfo.HouseCost * SellPriceMultiplier);
        var newPlayerState = playerState.AdjustCash(saleValue);

        // Update ownership via ReplaceExtras and player state via Next
        var stateWithOwnership = gameState.ReplaceExtras(newOwnership);

        return stateWithOwnership.Next([newPlayerState]);
    }
}
