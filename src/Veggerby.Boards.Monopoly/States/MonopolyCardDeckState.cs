using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Monopoly.Cards;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.States;

/// <summary>
/// Marker artifact for Monopoly card deck state.
/// </summary>
internal sealed class MonopolyCardDeckMarker : Artifact
{
    public MonopolyCardDeckMarker(string deckId) : base(deckId) { }
}

/// <summary>
/// Represents the state of a Monopoly card deck (Chance or Community Chest).
/// </summary>
public sealed class MonopolyCardDeckState : IArtifactState
{
    private readonly MonopolyCardDeckMarker _marker;

    /// <summary>
    /// Gets the deck identifier.
    /// </summary>
    public string DeckId { get; }

    /// <summary>
    /// Gets the cards in the draw pile (in order, top card is first).
    /// </summary>
    public IReadOnlyList<MonopolyCardDefinition> DrawPile { get; }

    /// <summary>
    /// Gets the cards in the discard pile.
    /// </summary>
    public IReadOnlyList<MonopolyCardDefinition> DiscardPile { get; }

    /// <inheritdoc />
    public Artifact Artifact => _marker;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonopolyCardDeckState"/> class.
    /// </summary>
    public MonopolyCardDeckState(
        string deckId,
        IEnumerable<MonopolyCardDefinition> drawPile,
        IEnumerable<MonopolyCardDefinition>? discardPile = null)
    {
        ArgumentNullException.ThrowIfNull(deckId);
        ArgumentNullException.ThrowIfNull(drawPile);

        _marker = new MonopolyCardDeckMarker(deckId);
        DeckId = deckId;
        DrawPile = drawPile.ToList().AsReadOnly();
        DiscardPile = discardPile?.ToList().AsReadOnly() ?? new List<MonopolyCardDefinition>().AsReadOnly();
    }

    /// <summary>
    /// Creates a new state with the top card drawn and placed in the discard pile.
    /// </summary>
    public (MonopolyCardDeckState NewState, MonopolyCardDefinition? DrawnCard) DrawCard()
    {
        if (DrawPile.Count == 0)
        {
            // Need to reshuffle - draw pile is empty
            return (this, null);
        }

        var drawnCard = DrawPile[0];
        var newDrawPile = DrawPile.Skip(1).ToList();
        var newDiscardPile = DiscardPile.Concat([drawnCard]).ToList();

        return (new MonopolyCardDeckState(DeckId, newDrawPile, newDiscardPile), drawnCard);
    }

    /// <summary>
    /// Creates a new state with the discard pile shuffled back into the draw pile.
    /// </summary>
    /// <param name="seed">Random seed for deterministic shuffling.</param>
    public MonopolyCardDeckState Reshuffle(int seed)
    {
        var allCards = DrawPile.Concat(DiscardPile).ToList();

        // Deterministic seeded shuffle using linear congruential generator
        // This avoids using System.Random which is banned
        uint state = (uint)seed;
        for (int i = allCards.Count - 1; i > 0; i--)
        {
            // Linear congruential generator step
            state = state * 1664525u + 1013904223u;
            int j = (int)(state % (uint)(i + 1));
            (allCards[i], allCards[j]) = (allCards[j], allCards[i]);
        }

        return new MonopolyCardDeckState(DeckId, allCards, []);
    }

    /// <summary>
    /// Creates a new state with a Get Out of Jail Free card removed (kept by player).
    /// </summary>
    public MonopolyCardDeckState RemoveGetOutOfJailCard()
    {
        var newDiscard = DiscardPile
            .Where(c => c.Effect != MonopolyCardEffect.GetOutOfJailFree)
            .ToList();

        return new MonopolyCardDeckState(DeckId, DrawPile, newDiscard);
    }

    /// <summary>
    /// Creates a new state with a Get Out of Jail Free card returned to the deck.
    /// </summary>
    public MonopolyCardDeckState ReturnGetOutOfJailCard(MonopolyCardDefinition card)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (card.Effect != MonopolyCardEffect.GetOutOfJailFree)
        {
            throw new ArgumentException("Card must be a Get Out of Jail Free card", nameof(card));
        }

        var newDiscard = DiscardPile.Concat([card]).ToList();
        return new MonopolyCardDeckState(DeckId, DrawPile, newDiscard);
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        if (other is not MonopolyCardDeckState otherState)
        {
            return false;
        }

        if (!string.Equals(DeckId, otherState.DeckId, StringComparison.Ordinal))
        {
            return false;
        }

        if (DrawPile.Count != otherState.DrawPile.Count ||
            DiscardPile.Count != otherState.DiscardPile.Count)
        {
            return false;
        }

        for (int i = 0; i < DrawPile.Count; i++)
        {
            if (!string.Equals(DrawPile[i].Id, otherState.DrawPile[i].Id, StringComparison.Ordinal))
            {
                return false;
            }
        }

        for (int i = 0; i < DiscardPile.Count; i++)
        {
            if (!string.Equals(DiscardPile[i].Id, otherState.DiscardPile[i].Id, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MonopolyCardDeckState state && Equals(state);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(DeckId);

        foreach (var card in DrawPile)
        {
            code.Add(card.Id);
        }

        foreach (var card in DiscardPile)
        {
            code.Add(card.Id);
        }

        return code.ToHashCode();
    }
}
