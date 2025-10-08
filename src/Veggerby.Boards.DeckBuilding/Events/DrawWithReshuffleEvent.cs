using System;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Draws a specified number of cards from the draw pile into the hand pile.
/// If the draw pile has insufficient cards, the discard pile is shuffled deterministically
/// and moved onto the draw pile before drawing. Fails if draw+discard still insufficient.
/// </summary>
public sealed class DrawWithReshuffleEvent : IGameEvent
{
    /// <summary>The deck to draw from.</summary>
    public Deck Deck { get; }

    /// <summary>The number of cards to draw.</summary>
    public int Count { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawWithReshuffleEvent"/> class.
    /// Uses standard DeckBuilding piles (draw, discard, hand).
    /// </summary>
    public DrawWithReshuffleEvent(Deck deck, int count)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        Count = count;
    }
}