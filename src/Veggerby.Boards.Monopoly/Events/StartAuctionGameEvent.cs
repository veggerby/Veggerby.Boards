using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing the start of a property auction.
/// </summary>
public class StartAuctionGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the position of the property being auctioned.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Gets the players eligible to participate in the auction.
    /// </summary>
    public IReadOnlyList<Player> EligiblePlayers
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StartAuctionGameEvent"/> class.
    /// </summary>
    public StartAuctionGameEvent(int propertyPosition, IEnumerable<Player> eligiblePlayers)
    {
        ArgumentNullException.ThrowIfNull(eligiblePlayers);

        PropertyPosition = propertyPosition;
        EligiblePlayers = new List<Player>(eligiblePlayers).AsReadOnly();
    }
}
