using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Cards.Mutators;

internal sealed class PeekCardsStateMutator : IStateMutator<PeekCardsEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, PeekCardsEvent @event)
    {
        // Peek is a read-only operation - no state change
        // Could optionally track peek action in extras for audit/visibility
        return state;
    }
}

internal sealed class RevealCardsStateMutator : IStateMutator<RevealCardsEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, RevealCardsEvent @event)
    {
        // Reveal marks cards as visible - for simplicity, this is a no-op mutator
        // Visibility tracking could be added to DeckState extras if needed
        return state;
    }
}

internal sealed class ReshuffleStateMutator : IStateMutator<ReshuffleEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, ReshuffleEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return state; // conditions should have prevented
        }

        if (!ds.Piles.TryGetValue(@event.FromPileId, out var fromPile))
        {
            throw new BoardException($"Unknown source pile '{@event.FromPileId}' for deck '{@event.Deck.Id}'.");
        }

        if (!ds.Piles.TryGetValue(@event.ToPileId, out var toPile))
        {
            throw new BoardException($"Unknown destination pile '{@event.ToPileId}' for deck '{@event.Deck.Id}'.");
        }

        // Combine piles: append from to current to
        var combined = new List<Card>(toPile.Count + fromPile.Count);
        combined.AddRange(toPile);
        combined.AddRange(fromPile);

        // Deterministic shuffle using Fisher-Yates
        var rng = state.Random;
        if (rng is not null && combined.Count > 1)
        {
            for (int i = combined.Count - 1; i > 0; i--)
            {
                var u = rng.NextUInt();
                var j = (int)(u % (uint)(i + 1));
                (combined[i], combined[j]) = (combined[j], combined[i]);
            }
        }

        // Build new piles
        var newPiles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            if (kv.Key == @event.ToPileId)
                newPiles[kv.Key] = combined;
            else if (kv.Key == @event.FromPileId)
                newPiles[kv.Key] = new List<Card>();
            else
                newPiles[kv.Key] = new List<Card>(kv.Value);
        }

        var next = new DeckState(ds.Artifact, newPiles, new Dictionary<string, int>(ds.Supply));
        return state.Next([next]);
    }
}
