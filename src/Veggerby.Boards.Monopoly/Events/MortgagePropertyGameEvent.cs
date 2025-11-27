using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player mortgaging a property.
/// </summary>
public class MortgagePropertyGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player mortgaging the property.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the position of the property being mortgaged.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MortgagePropertyGameEvent"/> class.
    /// </summary>
    public MortgagePropertyGameEvent(Player player, int propertyPosition)
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
