using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player buying a house on a property.
/// </summary>
public class BuyHouseGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player buying the house.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the position of the property to build on.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuyHouseGameEvent"/> class.
    /// </summary>
    public BuyHouseGameEvent(Player player, int propertyPosition)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        PropertyPosition = propertyPosition;
    }
}
