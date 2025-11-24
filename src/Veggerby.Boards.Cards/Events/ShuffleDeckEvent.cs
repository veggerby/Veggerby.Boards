using System;

using Veggerby.Boards.Flows.Events;

using Veggerby.Boards.Cards.Artifacts;
namespace Veggerby.Boards.Cards.Events;

/// <summary>
/// Event requesting a deterministic shuffle of a deck pile using the state's RNG snapshot.
/// </summary>
public sealed class ShuffleDeckEvent : IGameEvent, IStateMutationGameEvent
{
    /// <summary>Gets the deck artifact to shuffle.</summary>
    public Deck Deck
    {
        get;
    }
    /// <summary>Gets the pile identifier within the deck to shuffle.</summary>
    public string PileId
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleDeckEvent"/> class.
    /// </summary>
    /// <param name="deck">Deck artifact.</param>
    /// <param name="pileId">Pile id within the deck to shuffle.</param>
    public ShuffleDeckEvent(Deck deck, string pileId)
    {
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        PileId = pileId ?? throw new ArgumentNullException(nameof(pileId));
    }
}