using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player can unmortgage a property.
/// </summary>
public class CanUnmortgagePropertyCondition : IGameEventCondition<UnmortgagePropertyGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, UnmortgagePropertyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Only purchasable properties can be unmortgaged
        if (!squareInfo.IsPurchasable)
        {
            return ConditionResponse.Fail("Cannot unmortgage this square type");
        }

        // Check ownership
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return ConditionResponse.Ignore("Property ownership state not found");
        }

        // Check if player owns the property
        var owner = ownership.GetOwner(@event.PropertyPosition);
        if (!string.Equals(owner, @event.Player.Id, StringComparison.Ordinal))
        {
            return ConditionResponse.Fail("Player does not own this property");
        }

        // Check if property is mortgaged
        if (!ownership.IsMortgaged(@event.PropertyPosition))
        {
            return ConditionResponse.Fail("Property is not mortgaged");
        }

        // Check if player has enough cash
        // Unmortgage cost = mortgage value + 10% interest
        var mortgageValue = squareInfo.Price / 2;
        var unmortgageCost = mortgageValue + (mortgageValue / 10); // 10% interest

        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return ConditionResponse.Ignore("Player state not found");
        }

        if (playerState.Cash < unmortgageCost)
        {
            return ConditionResponse.Fail($"Insufficient funds - unmortgage costs ${unmortgageCost}, player has ${playerState.Cash}");
        }

        return ConditionResponse.Valid;
    }
}
