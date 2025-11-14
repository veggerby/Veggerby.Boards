using System;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Cards;

/// <summary>
/// Event requesting to move discard pile to draw pile and shuffle.
/// </summary>
public sealed class ReshuffleEvent : IGameEvent
{
    /// <summary>Gets the deck artifact.</summary>
    public Deck Deck
    {
        get;
    }

    /// <summary>Gets the source pile identifier (typically discard).</summary>
    public string FromPileId
    {
        get;
    }

    /// <summary>Gets the destination pile identifier (typically draw) to append and shuffle.</summary>
    public string ToPileId
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReshuffleEvent"/> class.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="fromPileId">Source pile id (discard).</param>
    /// <param name="toPileId">Destination pile id (draw) to shuffle into.</param>
    public ReshuffleEvent(Deck deck, string fromPileId, string toPileId)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        FromPileId = fromPileId ?? throw new ArgumentNullException(nameof(fromPileId));
        ToPileId = toPileId ?? throw new ArgumentNullException(nameof(toPileId));
    }
}
