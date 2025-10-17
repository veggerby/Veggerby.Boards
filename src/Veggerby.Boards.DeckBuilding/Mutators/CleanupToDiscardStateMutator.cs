using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

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
            throw new BoardException("Required piles missing");
        }
        var piles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            piles[kv.Key] = new List<Card>(kv.Value);
        }
        var hand = piles[handId];
        var inPlay = piles[inPlayId];
        var discard = piles[discardId];
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
        var next = new DeckState(ds.Artifact, piles, new Dictionary<string, int>(ds.Supply, StringComparer.Ordinal));
        return state.Next([next]);
    }
}