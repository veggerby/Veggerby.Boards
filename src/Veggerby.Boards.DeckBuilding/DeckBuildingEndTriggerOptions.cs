using System;
using System.Collections.Generic;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Configuration options for alternate deck-building end-game trigger based on supply depletion.
/// </summary>
public sealed class DeckBuildingEndTriggerOptions
{
    /// <summary>Gets the number of distinct empty supply piles required to permit game end (0 disables threshold logic).</summary>
    public int EmptySupplyPilesThreshold { get; }

    /// <summary>Gets the set of key pile card identifiers; if any are empty the end-game is permitted. Empty set disables key pile logic.</summary>
    public IReadOnlyCollection<string> KeyPileCardIds { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeckBuildingEndTriggerOptions"/> class.
    /// </summary>
    /// <param name="emptySupplyPilesThreshold">Number of empty piles required (0 disables threshold).</param>
    /// <param name="keyPileCardIds">Key pile card ids; emptiness of any permits end-game. Null treated as empty.</param>
    public DeckBuildingEndTriggerOptions(int emptySupplyPilesThreshold = 0, IReadOnlyCollection<string>? keyPileCardIds = null)
    {
        if (emptySupplyPilesThreshold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(emptySupplyPilesThreshold));
        }
        var keys = keyPileCardIds is null ? new List<string>() : new List<string>(keyPileCardIds);
        if (emptySupplyPilesThreshold <= 0 && keys.Count == 0)
        {
            throw new ArgumentException("At least one of: threshold > 0 or key pile ids non-empty must be supplied.");
        }
        EmptySupplyPilesThreshold = emptySupplyPilesThreshold;
        KeyPileCardIds = keys;
    }
}