using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Flows.Events;
namespace Veggerby.Boards.DeckBuilding.Events;

/// <summary>
/// Event representing trashing (removing from the game) specific cards from the Hand pile.
/// </summary>
public sealed class TrashFromHandEvent : IGameEvent
{
    /// <summary>Target deck.</summary>
    public Deck Deck
    {
        get;
    }

    /// <summary>Cards to trash; must be present in the Hand pile.</summary>
    public IReadOnlyList<Card> Cards
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrashFromHandEvent"/> class.
    /// </summary>
    public TrashFromHandEvent(Deck deck, IList<Card> cards)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        Cards = (cards ?? throw new ArgumentNullException(nameof(cards))).AsReadOnly();
    }
}