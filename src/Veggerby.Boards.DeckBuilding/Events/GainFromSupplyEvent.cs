using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Flows.Events;
namespace Veggerby.Boards.DeckBuilding.Events;

/// <summary>
/// Event representing a player gaining a specific card by id from the shared supply to a target pile in a deck.
/// </summary>
public sealed class GainFromSupplyEvent : IGameEvent
{
    /// <summary>Gets the player gaining the card.</summary>
    public Player Player
    {
        get;
    }

    /// <summary>Gets the deck receiving the gained card.</summary>
    public Deck Deck
    {
        get;
    }

    /// <summary>Gets the identifier of the card to gain (Card artifact id).</summary>
    public string CardId
    {
        get;
    }

    /// <summary>Gets the target pile identifier within the deck (e.g., discard).</summary>
    public string TargetPileId
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GainFromSupplyEvent"/> class.
    /// </summary>
    public GainFromSupplyEvent(Player player, Deck deck, string cardId, string targetPileId)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        ArgumentException.ThrowIfNullOrWhiteSpace(cardId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPileId);
        CardId = cardId;
        TargetPileId = targetPileId;
    }
}