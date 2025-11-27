using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player proposing a trade to another player.
/// </summary>
public class ProposeTradeGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player proposing the trade.
    /// </summary>
    public Player Proposer
    {
        get;
    }

    /// <summary>
    /// Gets the player receiving the trade proposal.
    /// </summary>
    public Player Target
    {
        get;
    }

    /// <summary>
    /// Gets the cash amount the proposer is offering.
    /// </summary>
    public int OfferedCash
    {
        get;
    }

    /// <summary>
    /// Gets the property positions the proposer is offering.
    /// </summary>
    public IReadOnlyList<int> OfferedProperties
    {
        get;
    }

    /// <summary>
    /// Gets whether the proposer is offering a Get Out of Jail Free card.
    /// </summary>
    public bool OfferedGetOutOfJailCard
    {
        get;
    }

    /// <summary>
    /// Gets the cash amount requested from the target.
    /// </summary>
    public int RequestedCash
    {
        get;
    }

    /// <summary>
    /// Gets the property positions requested from the target.
    /// </summary>
    public IReadOnlyList<int> RequestedProperties
    {
        get;
    }

    /// <summary>
    /// Gets whether a Get Out of Jail Free card is requested from the target.
    /// </summary>
    public bool RequestedGetOutOfJailCard
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProposeTradeGameEvent"/> class.
    /// </summary>
    public ProposeTradeGameEvent(
        Player proposer,
        Player target,
        int offeredCash = 0,
        IEnumerable<int>? offeredProperties = null,
        bool offeredGetOutOfJailCard = false,
        int requestedCash = 0,
        IEnumerable<int>? requestedProperties = null,
        bool requestedGetOutOfJailCard = false)
    {
        ArgumentNullException.ThrowIfNull(proposer);
        ArgumentNullException.ThrowIfNull(target);

        Proposer = proposer;
        Target = target;
        OfferedCash = offeredCash;
        OfferedProperties = (offeredProperties != null ? new List<int>(offeredProperties) : new List<int>()).AsReadOnly();
        OfferedGetOutOfJailCard = offeredGetOutOfJailCard;
        RequestedCash = requestedCash;
        RequestedProperties = (requestedProperties != null ? new List<int>(requestedProperties) : new List<int>()).AsReadOnly();
        RequestedGetOutOfJailCard = requestedGetOutOfJailCard;
    }
}
