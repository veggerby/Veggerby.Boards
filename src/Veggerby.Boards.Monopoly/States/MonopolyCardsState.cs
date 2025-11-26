using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Monopoly.Cards;

namespace Veggerby.Boards.Monopoly.States;

/// <summary>
/// State containing all card decks for the Monopoly game (Chance and Community Chest).
/// </summary>
public sealed class MonopolyCardsState
{
    /// <summary>
    /// Gets the Chance deck state.
    /// </summary>
    public MonopolyCardDeckState ChanceDeck { get; }

    /// <summary>
    /// Gets the Community Chest deck state.
    /// </summary>
    public MonopolyCardDeckState CommunityChestDeck { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonopolyCardsState"/> class.
    /// </summary>
    public MonopolyCardsState(MonopolyCardDeckState chanceDeck, MonopolyCardDeckState communityChestDeck)
    {
        ArgumentNullException.ThrowIfNull(chanceDeck);
        ArgumentNullException.ThrowIfNull(communityChestDeck);

        ChanceDeck = chanceDeck;
        CommunityChestDeck = communityChestDeck;
    }

    /// <summary>
    /// Gets the deck by identifier.
    /// </summary>
    public MonopolyCardDeckState? GetDeck(string deckId)
    {
        if (string.Equals(deckId, MonopolyCardDecks.ChanceDeckId, StringComparison.Ordinal))
        {
            return ChanceDeck;
        }

        if (string.Equals(deckId, MonopolyCardDecks.CommunityChestDeckId, StringComparison.Ordinal))
        {
            return CommunityChestDeck;
        }

        return null;
    }

    /// <summary>
    /// Creates a new state with an updated deck.
    /// </summary>
    public MonopolyCardsState WithUpdatedDeck(MonopolyCardDeckState deck)
    {
        ArgumentNullException.ThrowIfNull(deck);

        if (string.Equals(deck.DeckId, MonopolyCardDecks.ChanceDeckId, StringComparison.Ordinal))
        {
            return new MonopolyCardsState(deck, CommunityChestDeck);
        }

        if (string.Equals(deck.DeckId, MonopolyCardDecks.CommunityChestDeckId, StringComparison.Ordinal))
        {
            return new MonopolyCardsState(ChanceDeck, deck);
        }

        return this;
    }

    /// <summary>
    /// Gets all decks as enumerable.
    /// </summary>
    public IEnumerable<MonopolyCardDeckState> GetAllDecks()
    {
        yield return ChanceDeck;
        yield return CommunityChestDeck;
    }
}
