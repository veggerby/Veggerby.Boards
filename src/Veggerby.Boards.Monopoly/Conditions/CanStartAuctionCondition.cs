using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if an auction can be started for a property.
/// </summary>
public class CanStartAuctionCondition : IGameEventCondition<StartAuctionGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, StartAuctionGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var squareInfo = MonopolyBoardConfiguration.GetSquare(@event.PropertyPosition);

        // Can only auction properties
        if (squareInfo.SquareType != SquareType.Property &&
            squareInfo.SquareType != SquareType.Railroad &&
            squareInfo.SquareType != SquareType.Utility)
        {
            return ConditionResponse.Fail("Can only auction properties, railroads, or utilities");
        }

        // Get current ownership state
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return ConditionResponse.Ignore("Property ownership state not found");
        }

        // Property must be unowned
        if (ownership.IsOwned(@event.PropertyPosition))
        {
            return ConditionResponse.Fail("Property is already owned");
        }

        // No auction can be in progress
        var auctionState = state.GetExtras<AuctionState>();
        if (auctionState is not null && auctionState.IsActive)
        {
            return ConditionResponse.Fail("An auction is already in progress");
        }

        // Must have at least one eligible player
        if (@event.EligiblePlayers.Count == 0)
        {
            return ConditionResponse.Fail("No eligible players for auction");
        }

        return ConditionResponse.Valid;
    }
}
