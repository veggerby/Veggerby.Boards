using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;

using Veggerby.Boards.Cards.Artifacts;
namespace Veggerby.Boards.Cards.Events;

/// <summary>
/// Event requesting to make specific cards visible to all players.
/// </summary>
public sealed class RevealCardsEvent : IGameEvent
{
    /// <summary>Gets the deck artifact.</summary>
    public Deck Deck
    {
        get;
    }

    /// <summary>Gets the pile identifier containing the cards to reveal.</summary>
    public string PileId
    {
        get;
    }

    /// <summary>Gets the ordered list of cards to reveal.</summary>
    public IReadOnlyList<Card> Cards
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RevealCardsEvent"/> class.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="pileId">Pile id containing the cards.</param>
    /// <param name="cards">Cards to reveal.</param>
    public RevealCardsEvent(Deck deck, string pileId, IReadOnlyList<Card> cards)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        PileId = pileId ?? throw new ArgumentNullException(nameof(pileId));
        Cards = cards ?? throw new ArgumentNullException(nameof(cards));
    }
}
