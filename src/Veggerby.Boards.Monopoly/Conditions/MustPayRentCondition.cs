using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that validates a player must pay rent.
/// </summary>
public class MustPayRentCondition : IGameEventCondition<PayRentGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, PayRentGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var position = @event.PropertyPosition;
        var squareInfo = MonopolyBoardConfiguration.GetSquare(position);

        // Check if property is rentable
        if (!squareInfo.IsPurchasable)
        {
            return ConditionResponse.Ignore($"Square at position {position} is not a rentable property");
        }

        // Check if property is owned
        var ownership = state.GetStates<PropertyOwnershipState>().FirstOrDefault();
        if (ownership is null)
        {
            return ConditionResponse.Ignore("Property ownership state not found");
        }

        var ownerId = ownership.GetOwner(position);
        if (ownerId is null)
        {
            return ConditionResponse.Ignore($"Property at position {position} is not owned");
        }

        // Check that payer is not the owner
        if (string.Equals(ownerId, @event.Payer.Id, StringComparison.Ordinal))
        {
            return ConditionResponse.Ignore("Player landed on their own property");
        }

        // Check that owner is not bankrupt
        var ownerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, ownerId, StringComparison.Ordinal));

        if (ownerState is not null && ownerState.IsBankrupt)
        {
            return ConditionResponse.Ignore("Property owner is bankrupt");
        }

        // Check that owner is not in jail (optional rule - we'll include it)
        if (ownerState is not null && ownerState.InJail)
        {
            return ConditionResponse.Ignore("Property owner is in jail");
        }

        return ConditionResponse.Valid;
    }
}
