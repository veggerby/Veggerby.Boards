using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that validates a property can be purchased.
/// </summary>
public class CanBuyPropertyCondition : IGameEventCondition<BuyPropertyGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, BuyPropertyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var position = @event.PropertyPosition;
        var squareInfo = MonopolyBoardConfiguration.GetSquare(position);

        // Check if property is purchasable
        if (!squareInfo.IsPurchasable)
        {
            return ConditionResponse.Ignore($"Square at position {position} is not purchasable");
        }

        // Check if property is already owned
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return ConditionResponse.Ignore("Property ownership state not found");
        }

        if (ownership.IsOwned(position))
        {
            return ConditionResponse.Fail($"Property at position {position} is already owned");
        }

        // Check if player has enough cash
        var playerState = state.GetExtras<MonopolyPlayerState>();
        if (playerState is null)
        {
            return ConditionResponse.Ignore($"Player state not found for {@event.Player.Id}");
        }

        if (!string.Equals(playerState.Player.Id, @event.Player.Id, StringComparison.Ordinal))
        {
            return ConditionResponse.Ignore($"Player state mismatch for {@event.Player.Id}");
        }

        if (playerState.Cash < squareInfo.Price)
        {
            return ConditionResponse.Fail($"Player {@event.Player.Id} has insufficient funds (${playerState.Cash}) to buy property (${squareInfo.Price})");
        }

        return ConditionResponse.Valid;
    }
}
