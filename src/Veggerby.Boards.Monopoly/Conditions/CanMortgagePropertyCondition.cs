using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player can mortgage a property.
/// </summary>
public class CanMortgagePropertyCondition : IGameEventCondition<MortgagePropertyGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MortgagePropertyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Only purchasable properties can be mortgaged
        if (!squareInfo.IsPurchasable)
        {
            return ConditionResponse.Fail("Cannot mortgage this square type");
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

        // Check if already mortgaged
        if (ownership.IsMortgaged(@event.PropertyPosition))
        {
            return ConditionResponse.Fail("Property is already mortgaged");
        }

        // Check if there are houses on this property
        if (ownership.GetHouseCount(@event.PropertyPosition) > 0)
        {
            return ConditionResponse.Fail("Cannot mortgage property with houses - sell houses first");
        }

        // Check if any property in the color group has houses (if it's a colored property)
        if (squareInfo.CanBuildHouses)
        {
            var maxHouses = ownership.GetMaxHouseCountInColorGroup(@event.Player.Id, squareInfo.ColorGroup);
            if (maxHouses > 0)
            {
                return ConditionResponse.Fail("Cannot mortgage property while any property in the color group has houses");
            }
        }

        return ConditionResponse.Valid;
    }
}
