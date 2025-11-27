using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player selling a house from a property.
/// </summary>
public class SellHouseGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player selling the house.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the position of the property to sell from.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SellHouseGameEvent"/> class.
    /// </summary>
    public SellHouseGameEvent(Player player, int propertyPosition)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        PropertyPosition = propertyPosition;
    }
}
