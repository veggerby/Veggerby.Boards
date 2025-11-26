using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player drawing a card from Chance or Community Chest.
/// </summary>
public class DrawCardGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player drawing the card.
    /// </summary>
    public Player Player { get; }

    /// <summary>
    /// Gets the deck identifier ("chance-deck" or "community-chest-deck").
    /// </summary>
    public string DeckId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawCardGameEvent"/> class.
    /// </summary>
    public DrawCardGameEvent(Player player, string deckId)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(deckId);

        Player = player;
        DeckId = deckId;
    }
}
