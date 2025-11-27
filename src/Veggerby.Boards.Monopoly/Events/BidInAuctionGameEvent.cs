using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player bidding in an auction.
/// </summary>
public class BidInAuctionGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player placing the bid.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the bid amount.
    /// </summary>
    public int BidAmount
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BidInAuctionGameEvent"/> class.
    /// </summary>
    public BidInAuctionGameEvent(Player player, int bidAmount)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (bidAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bidAmount), "Bid amount must be positive");
        }

        Player = player;
        BidAmount = bidAmount;
    }
}
