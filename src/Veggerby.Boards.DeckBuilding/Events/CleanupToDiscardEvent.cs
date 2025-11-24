using System;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Events;

using Veggerby.Boards.Cards.Artifacts;
namespace Veggerby.Boards.DeckBuilding.Events;

/// <summary>
/// End-of-turn cleanup event: moves all cards from Hand and InPlay into Discard.
/// </summary>
public sealed class CleanupToDiscardEvent : IGameEvent
{
    /// <summary>The deck to clean up.</summary>
    public Deck Deck
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanupToDiscardEvent"/> class.
    /// </summary>
    public CleanupToDiscardEvent(Deck deck)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
    }
}