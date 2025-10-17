using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Removes specified cards from the Hand pile and returns a new DeckState.
/// </summary>
public sealed class TrashFromHandStateMutator : IStateMutator<TrashFromHandEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, TrashFromHandEvent @event)
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
        if (!ds.Piles.ContainsKey(handId))
        {
            throw new BoardException("Hand pile missing");
        }
        // clone piles
        var piles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            piles[kv.Key] = new List<Card>(kv.Value);
        }
        var hand = piles[handId];
        // remove each specified card by identity
        foreach (var c in @event.Cards)
        {
            var idx = hand.IndexOf(c);
            if (idx >= 0)
            {
                hand.RemoveAt(idx);
            }
        }
        var next = new DeckState(ds.Artifact, piles, new Dictionary<string, int>(ds.Supply, StringComparer.Ordinal));
        return state.Next([next]);
    }
}