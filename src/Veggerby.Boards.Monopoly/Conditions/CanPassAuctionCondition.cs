using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player can pass on an auction.
/// </summary>
public class CanPassAuctionCondition : IGameEventCondition<PassAuctionGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, PassAuctionGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Get auction state
        var auctionState = state.GetExtras<AuctionState>();
        if (auctionState is null || !auctionState.IsActive)
        {
            return ConditionResponse.Fail("No active auction");
        }

        // Player must be eligible to participate
        if (!auctionState.CanPlayerBid(@event.Player.Id))
        {
            return ConditionResponse.Fail("Player is not eligible to participate in this auction");
        }

        return ConditionResponse.Valid;
    }
}
