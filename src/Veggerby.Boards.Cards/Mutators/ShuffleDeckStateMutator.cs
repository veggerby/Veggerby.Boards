using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
namespace Veggerby.Boards.Cards.Mutators;

internal sealed class ShuffleDeckStateMutator : IStateMutator<ShuffleDeckEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, ShuffleDeckEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return state; // conditions should have prevented
        }

        if (!ds.Piles.TryGetValue(@event.PileId, out var list))
        {
            throw new BoardException($"Unknown pile '{@event.PileId}' for deck '{@event.Deck.Id}'.");
        }

        var arr = new List<CardState>(list);
        var rng = state.Random;
        if (rng is null || arr.Count <= 1)
        {
            // Determinism: without RNG snapshot, don't reorder.
            return state; // no-op
        }

        // Fisher-Yates with deterministic IRandomSource
        for (int i = arr.Count - 1; i > 0; i--)
        {
            // Use NextUInt to pick index
            var u = rng.NextUInt();
            var j = (int)(u % (uint)(i + 1));
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        var newPiles = new Dictionary<string, IList<CardState>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            newPiles[kv.Key] = kv.Key == @event.PileId ? arr : new List<CardState>(kv.Value);
        }

        var next = new DeckState(ds.Artifact, newPiles, new Dictionary<string, int>(ds.Supply));
        return state.Next([next]);
    }
}