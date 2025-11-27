using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player paying rent to a property owner.
/// </summary>
public class PayRentGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player paying rent.
    /// </summary>
    public Player Payer
    {
        get;
    }

    /// <summary>
    /// Gets the property position where rent is being paid.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Gets the dice roll total (used for utility rent calculation).
    /// </summary>
    public int DiceTotal
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PayRentGameEvent"/> class.
    /// </summary>
    public PayRentGameEvent(Player payer, int propertyPosition, int diceTotal = 0)
    {
        ArgumentNullException.ThrowIfNull(payer);

        Payer = payer;
        PropertyPosition = propertyPosition;
        DiceTotal = diceTotal;
    }
}
