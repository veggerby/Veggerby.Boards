using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player buying a property.
/// </summary>
public class BuyPropertyGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player buying the property.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the position of the property being purchased.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuyPropertyGameEvent"/> class.
    /// </summary>
    public BuyPropertyGameEvent(Player player, int propertyPosition)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        PropertyPosition = propertyPosition;
    }
}
