using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if a player can bid in an auction.
/// </summary>
public class CanBidInAuctionCondition : IGameEventCondition<BidInAuctionGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, BidInAuctionGameEvent @event)
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

        // Player must be eligible to bid
        if (!auctionState.CanPlayerBid(@event.Player.Id))
        {
            return ConditionResponse.Fail("Player is not eligible to bid");
        }

        // Bid must be higher than current bid
        if (@event.BidAmount <= auctionState.CurrentBid)
        {
            return ConditionResponse.Fail($"Bid must be higher than current bid of ${auctionState.CurrentBid}");
        }

        // Player must have sufficient cash
        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return ConditionResponse.Ignore("Player state not found");
        }

        if (playerState.Cash < @event.BidAmount)
        {
            return ConditionResponse.Fail("Insufficient funds for bid");
        }

        return ConditionResponse.Valid;
    }
}
