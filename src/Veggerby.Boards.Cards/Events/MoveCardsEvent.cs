using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Flows.Events;
namespace Veggerby.Boards.Cards.Events;

/// <summary>
/// Event moving cards from a source pile to a destination pile by count or explicit identities.
/// </summary>
public sealed class MoveCardsEvent : IGameEvent
{
    /// <summary>Gets the deck artifact.</summary>
    public Deck Deck
    {
        get;
    }
    /// <summary>Gets the source pile identifier.</summary>
    public string FromPileId
    {
        get;
    }
    /// <summary>Gets the destination pile identifier.</summary>
    public string ToPileId
    {
        get;
    }
    /// <summary>Gets the number of cards to move when moving by count; otherwise null.</summary>
    public int? Count
    {
        get;
    }
    /// <summary>Gets the explicit list of cards to move; otherwise null when moving by count.</summary>
    public IReadOnlyList<Card>? Cards
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveCardsEvent"/> class to move by count.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="fromPileId">Source pile id.</param>
    /// <param name="toPileId">Destination pile id.</param>
    /// <param name="count">Number of cards to move.</param>
    public MoveCardsEvent(Deck deck, string fromPileId, string toPileId, int count)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        FromPileId = fromPileId ?? throw new ArgumentNullException(nameof(fromPileId));
        ToPileId = toPileId ?? throw new ArgumentNullException(nameof(toPileId));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        Count = count;
        Cards = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveCardsEvent"/> class to move explicit cards.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="fromPileId">Source pile id.</param>
    /// <param name="toPileId">Destination pile id.</param>
    /// <param name="cards">Cards to move.</param>
    public MoveCardsEvent(Deck deck, string fromPileId, string toPileId, IReadOnlyList<Card> cards)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        FromPileId = fromPileId ?? throw new ArgumentNullException(nameof(fromPileId));
        ToPileId = toPileId ?? throw new ArgumentNullException(nameof(toPileId));
        Cards = cards ?? throw new ArgumentNullException(nameof(cards));
        Count = null;
    }
}