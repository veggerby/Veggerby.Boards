using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Aggregated supply statistics to avoid repeated dictionary scans for depletion checks.
/// Maintained incrementally by deck-building mutators.
/// </summary>
public sealed class DeckSupplyStats
{
    /// <summary>Gets total distinct supply pile count.</summary>
    public int TotalPiles { get; }
    /// <summary>Gets current empty supply pile count.</summary>
    public int EmptyPiles { get; }

    /// <summary>
    /// Initializes a new instance aggregating supply statistics.
    /// </summary>
    /// <param name="totalPiles">Total distinct supply entries.</param>
    /// <param name="emptyPiles">Number of entries whose count is &lt;= 0.</param>
    public DeckSupplyStats(int totalPiles, int emptyPiles)
    {
        if (totalPiles < 0) throw new ArgumentOutOfRangeException(nameof(totalPiles));
        if (emptyPiles < 0 || emptyPiles > totalPiles) throw new ArgumentOutOfRangeException(nameof(emptyPiles));
        TotalPiles = totalPiles;
        EmptyPiles = emptyPiles;
    }

    /// <summary>Creates stats from a raw supply dictionary.</summary>
    public static DeckSupplyStats From(IDictionary<string, int> supply)
    {
        if (supply is null || supply.Count == 0) return new DeckSupplyStats(0, 0);
        var empty = 0;
        foreach (var kv in supply)
        {
            if (kv.Value <= 0) { empty++; }
        }
        return new DeckSupplyStats(supply.Count, empty);
    }

    /// <summary>Adjusts stats after decrement of a single supply entry; count may reach zero.</summary>
    public DeckSupplyStats AfterDecrement(int previousValue, int newValue)
    {
        // transition to zero increments empty count once
        if (previousValue > 0 && newValue <= 0)
        {
            return new DeckSupplyStats(TotalPiles, EmptyPiles + 1);
        }
        return this; // no change otherwise
    }
}