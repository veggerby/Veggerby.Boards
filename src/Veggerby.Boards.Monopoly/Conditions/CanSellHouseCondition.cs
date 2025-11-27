using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player can sell a house from a property.
/// </summary>
public class CanSellHouseCondition : IGameEventCondition<SellHouseGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, SellHouseGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Can only sell from colored properties
        if (squareInfo.SquareType != SquareType.Property)
        {
            return ConditionResponse.Fail("Can only sell houses from colored properties");
        }

        // Get current ownership state
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return ConditionResponse.Ignore("Property ownership state not found");
        }

        // Must own the property
        var owner = ownership.GetOwner(@event.PropertyPosition);
        if (!string.Equals(owner, @event.Player.Id, StringComparison.Ordinal))
        {
            return ConditionResponse.Fail("Player does not own this property");
        }

        // Must have houses to sell
        var houseCount = ownership.GetHouseCount(@event.PropertyPosition);
        if (houseCount <= 0)
        {
            return ConditionResponse.Fail("Property has no houses to sell");
        }

        // Even selling rule: cannot sell if other properties in group have fewer houses
        var maxInGroup = ownership.GetMaxHouseCountInColorGroup(@event.Player.Id, squareInfo.ColorGroup);
        if (houseCount < maxInGroup)
        {
            return ConditionResponse.Fail("Must sell houses evenly - other properties have more houses");
        }

        return ConditionResponse.Valid;
    }
}
