using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles a player bidding in an auction.
/// </summary>
public class BidInAuctionStateMutator : IStateMutator<BidInAuctionGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, BidInAuctionGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Get auction state
        var auctionState = gameState.GetExtras<AuctionState>()
            ?? throw new InvalidOperationException("No auction state found");

        // Place the bid
        var newAuctionState = auctionState.PlaceBid(@event.Player.Id, @event.BidAmount);

        return gameState.ReplaceExtras(newAuctionState);
    }
}
