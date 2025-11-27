using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles a player passing on an auction.
/// </summary>
public class PassAuctionStateMutator : IStateMutator<PassAuctionGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, PassAuctionGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Get auction state
        var auctionState = gameState.GetExtras<AuctionState>()
            ?? throw new InvalidOperationException("No auction state found");

        // Record the pass
        var newAuctionState = auctionState.PassBid(@event.Player.Id);

        // Check if auction should end
        if (newAuctionState.ShouldEnd())
        {
            // If there was a winning bid, transfer the property
            if (newAuctionState.HighestBidderId is not null && newAuctionState.CurrentBid > 0)
            {
                // Get ownership state
                var ownership = gameState.GetExtras<PropertyOwnershipState>()
                    ?? new PropertyOwnershipState();

                // Transfer property to winner
                var newOwnership = ownership.SetOwner(newAuctionState.PropertyPosition, newAuctionState.HighestBidderId);

                // Deduct cash from winner
                var winnerState = gameState.GetStates<MonopolyPlayerState>()
                    .FirstOrDefault(ps => string.Equals(ps.Player.Id, newAuctionState.HighestBidderId, StringComparison.Ordinal));

                if (winnerState is not null)
                {
                    var newWinnerState = winnerState.AdjustCash(-newAuctionState.CurrentBid);

                    // End auction and update all states
                    var endedAuction = newAuctionState.EndAuction();

                    return gameState
                        .ReplaceExtras(endedAuction)
                        .ReplaceExtras(newOwnership)
                        .Next([newWinnerState]);
                }
            }

            // No winner - just end the auction
            return gameState.ReplaceExtras(newAuctionState.EndAuction());
        }

        return gameState.ReplaceExtras(newAuctionState);
    }
}
