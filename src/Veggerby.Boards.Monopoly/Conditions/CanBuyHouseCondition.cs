using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player can buy a house on a property.
/// </summary>
public class CanBuyHouseCondition : IGameEventCondition<BuyHouseGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, BuyHouseGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Only colored properties can have houses
        if (squareInfo.SquareType != SquareType.Property)
        {
            return ConditionResponse.Fail("Can only build houses on colored properties");
        }

        if (!squareInfo.CanBuildHouses)
        {
            return ConditionResponse.Fail($"Cannot build houses on {squareInfo.Name}");
        }

        // Check ownership
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return ConditionResponse.Ignore("Property ownership state not found");
        }

        var owner = ownership.GetOwner(@event.PropertyPosition);
        if (!string.Equals(owner, @event.Player.Id, StringComparison.Ordinal))
        {
            return ConditionResponse.Fail("Player does not own this property");
        }

        // Check monopoly requirement
        if (!ownership.HasMonopoly(@event.Player.Id, squareInfo.ColorGroup))
        {
            return ConditionResponse.Fail("Player must own all properties in the color group to build houses");
        }

        // Check even building rule
        if (!ownership.CanBuildHouse(@event.PropertyPosition, @event.Player.Id, squareInfo.ColorGroup))
        {
            return ConditionResponse.Fail("Cannot build house here due to even building rule - build on properties with fewer houses first");
        }

        // Check player has enough cash
        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return ConditionResponse.Ignore("Player state not found");
        }

        if (playerState.Cash < squareInfo.HouseCost)
        {
            return ConditionResponse.Fail($"Insufficient funds - house costs ${squareInfo.HouseCost}, player has ${playerState.Cash}");
        }

        return ConditionResponse.Valid;
    }
}
