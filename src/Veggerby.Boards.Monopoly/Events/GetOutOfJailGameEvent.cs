using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Defines how a player gets out of jail.
/// </summary>
public enum GetOutOfJailMethod
{
    /// <summary>
    /// Rolled doubles to get out.
    /// </summary>
    RolledDoubles,

    /// <summary>
    /// Paid the $50 fine.
    /// </summary>
    PaidFine,

    /// <summary>
    /// Used a Get Out of Jail Free card.
    /// </summary>
    UsedCard
}

/// <summary>
/// Event representing a player getting out of jail.
/// </summary>
public class GetOutOfJailGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player getting out of jail.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the method used to get out of jail.
    /// </summary>
    public GetOutOfJailMethod Method
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOutOfJailGameEvent"/> class.
    /// </summary>
    public GetOutOfJailGameEvent(Player player, GetOutOfJailMethod method)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        Method = method;
    }
}
