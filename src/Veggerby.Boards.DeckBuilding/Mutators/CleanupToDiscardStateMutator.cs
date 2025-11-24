using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.Internal;
namespace Veggerby.Boards.DeckBuilding.Mutators;

/// <summary>
/// Appends Hand and InPlay piles to Discard and empties the sources.
/// </summary>
public sealed class CleanupToDiscardStateMutator : IStateMutator<CleanupToDiscardEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, CleanupToDiscardEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return state;
        }
        var handId = DeckBuildingGameBuilder.Piles.Hand;
        var inPlayId = DeckBuildingGameBuilder.Piles.InPlay;
        var discardId = DeckBuildingGameBuilder.Piles.Discard;
        if (!ds.Piles.ContainsKey(handId) || !ds.Piles.ContainsKey(inPlayId) || !ds.Piles.ContainsKey(discardId))
        {
            throw new BoardException(ExceptionMessages.RequiredPilesMissing);
        }
        // Optimized selective cloning: only Hand, InPlay, and Discard piles need mutation.
        var piles = new Dictionary<string, IList<Card>>(ds.Piles.Count, StringComparer.Ordinal);
        // Prepare mutable working copies for the three mutated piles; other piles reused.
        var originalHand = ds.Piles[handId];
        var originalInPlay = ds.Piles[inPlayId];
        var originalDiscard = ds.Piles[discardId];
        var hand = originalHand.Count == 0 ? new List<Card>(capacity: 0) : new List<Card>(originalHand);
        var inPlay = originalInPlay.Count == 0 ? new List<Card>(capacity: 0) : new List<Card>(originalInPlay);
        var discard = originalDiscard.Count == 0 ? new List<Card>(capacity: 0) : new List<Card>(originalDiscard);
        foreach (var kv in ds.Piles)
        {
            if (kv.Key.Equals(handId, StringComparison.Ordinal))
            {
                piles[kv.Key] = hand;
            }
            else if (kv.Key.Equals(inPlayId, StringComparison.Ordinal))
            {
                piles[kv.Key] = inPlay;
            }
            else if (kv.Key.Equals(discardId, StringComparison.Ordinal))
            {
                piles[kv.Key] = discard;
            }
            else
            {
                piles[kv.Key] = (IList<Card>)kv.Value; // reuse existing read-only list
            }
        }
        if (hand.Count == 0 && inPlay.Count == 0)
        {
            return state; // no-op
        }
        for (var i = 0; i < hand.Count; i++)
        {
            discard.Add(hand[i]);
        }
        for (var i = 0; i < inPlay.Count; i++)
        {
            discard.Add(inPlay[i]);
        }
        hand.Clear();
        inPlay.Clear();
        var next = new DeckState(ds.Artifact, piles, ds.Supply is null ? null : new Dictionary<string, int>(ds.Supply, StringComparer.Ordinal));
        return state.Next([next]);
    }
}