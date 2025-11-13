using System;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Cards;

/// <summary>
/// Event requesting to draw a number of cards from a source pile into a destination pile.
/// </summary>
public sealed class DrawCardsEvent : IGameEvent
{
    /// <summary>Gets the deck artifact.</summary>
    public Deck Deck
    {
        get;
    }
    /// <summary>Gets the source pile identifier to draw from.</summary>
    public string FromPileId
    {
        get;
    }
    /// <summary>Gets the destination pile identifier to append drawn cards to.</summary>
    public string ToPileId
    {
        get;
    }
    /// <summary>Gets the number of cards to draw.</summary>
    public int Count
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawCardsEvent"/> class.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="fromPileId">Source pile id.</param>
    /// <param name="toPileId">Destination pile id.</param>
    /// <param name="count">Number of cards to draw.</param>
    public DrawCardsEvent(Deck deck, string fromPileId, string toPileId, int count)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        FromPileId = fromPileId ?? throw new ArgumentNullException(nameof(fromPileId));
        ToPileId = toPileId ?? throw new ArgumentNullException(nameof(toPileId));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        Count = count;
    }
}