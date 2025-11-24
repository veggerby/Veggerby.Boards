using System;

using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Flows.Events;
namespace Veggerby.Boards.Cards.Events;

/// <summary>
/// Event requesting to view top N cards from a pile without removing them.
/// </summary>
public sealed class PeekCardsEvent : IGameEvent
{
    /// <summary>Gets the deck artifact.</summary>
    public Deck Deck
    {
        get;
    }

    /// <summary>Gets the pile identifier to peek into.</summary>
    public string PileId
    {
        get;
    }

    /// <summary>Gets the number of cards to peek at.</summary>
    public int Count
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PeekCardsEvent"/> class.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="pileId">Pile id to peek into.</param>
    /// <param name="count">Number of cards to peek at.</param>
    public PeekCardsEvent(Deck deck, string pileId, int count)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        PileId = pileId ?? throw new ArgumentNullException(nameof(pileId));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        Count = count;
    }
}
