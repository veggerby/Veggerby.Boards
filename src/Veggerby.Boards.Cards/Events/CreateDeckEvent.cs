using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;

using Veggerby.Boards.Cards.Artifacts;
namespace Veggerby.Boards.Cards.Events;

/// <summary>
/// Event declaring the initial deck state with piles and optional supply.
/// </summary>
public sealed class CreateDeckEvent : IGameEvent
{
    /// <summary>
    /// Gets the deck artifact this event initializes.
    /// </summary>
    public Deck Deck
    {
        get;
    }
    /// <summary>
    /// Gets the initial ordered piles mapping pile id to list of cards.
    /// </summary>
    public IDictionary<string, IList<Card>> Piles
    {
        get;
    }
    /// <summary>
    /// Gets the optional supply counts by card identifier (for games with a shared supply).
    /// </summary>
    public IDictionary<string, int>? Supply
    {
        get;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDeckEvent"/> class.
    /// </summary>
    /// <param name="deck">Deck artifact to initialize.</param>
    /// <param name="piles">Initial piles mapping pile id to ordered cards.</param>
    /// <param name="supply">Optional supply counts by card id.</param>
    public CreateDeckEvent(Deck deck, IDictionary<string, IList<Card>> piles, IDictionary<string, int>? supply = null)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        Piles = piles ?? throw new ArgumentNullException(nameof(piles));
        Supply = supply; // optional
    }
}