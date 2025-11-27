using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player unmortgaging (lifting the mortgage on) a property.
/// </summary>
public class UnmortgagePropertyGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player unmortgaging the property.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the position of the property being unmortgaged.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnmortgagePropertyGameEvent"/> class.
    /// </summary>
    public UnmortgagePropertyGameEvent(Player player, int propertyPosition)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (propertyPosition < 0 || propertyPosition >= 40)
        {
            throw new ArgumentOutOfRangeException(nameof(propertyPosition), "Property position must be between 0 and 39");
        }

        Player = player;
        PropertyPosition = propertyPosition;
    }
}
