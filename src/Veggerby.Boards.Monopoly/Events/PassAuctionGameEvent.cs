using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player passing on an auction.
/// </summary>
public class PassAuctionGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player passing on the auction.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PassAuctionGameEvent"/> class.
    /// </summary>
    public PassAuctionGameEvent(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
    }
}
