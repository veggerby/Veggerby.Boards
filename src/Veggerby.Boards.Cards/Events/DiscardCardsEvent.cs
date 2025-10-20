using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Cards;

/// <summary>
/// Event moving specific cards into a destination pile (e.g., discard).
/// </summary>
public sealed class DiscardCardsEvent : IGameEvent
{
    /// <summary>Gets the deck artifact.</summary>
    public Deck Deck
    {
        get;
    }
    /// <summary>Gets the destination pile identifier.</summary>
    public string ToPileId
    {
        get;
    }
    /// <summary>Gets the ordered list of cards to move into the destination pile.</summary>
    public IReadOnlyList<Card> Cards
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscardCardsEvent"/> class.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="toPileId">Destination pile id.</param>
    /// <param name="cards">Cards to move in order.</param>
    public DiscardCardsEvent(Deck deck, string toPileId, IReadOnlyList<Card> cards)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        ToPileId = toPileId ?? throw new ArgumentNullException(nameof(toPileId));
        Cards = cards ?? throw new ArgumentNullException(nameof(cards));
    }
}