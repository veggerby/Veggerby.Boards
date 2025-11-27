using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player paying taxes.
/// </summary>
public class PayTaxGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player paying tax.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the tax amount.
    /// </summary>
    public int Amount
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PayTaxGameEvent"/> class.
    /// </summary>
    public PayTaxGameEvent(Player player, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Tax amount must be non-negative");
        }

        Player = player;
        Amount = amount;
    }
}
