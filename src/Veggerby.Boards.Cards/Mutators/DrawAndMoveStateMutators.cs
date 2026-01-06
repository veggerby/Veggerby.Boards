using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Cards.Mutators;

internal sealed class DrawCardsStateMutator : IStateMutator<DrawCardsEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, DrawCardsEvent @event)
    {
        if (@event.Count == 0)
        {
            return state; // no-op
        }

        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return state; // conditions should have prevented, no-op
        }

        if (!ds.Piles.ContainsKey(@event.FromPileId) || !ds.Piles.ContainsKey(@event.ToPileId))
        {
            throw new BoardException("Unknown pile specified.");
        }

        var from = ds.Piles[@event.FromPileId];
        var to = ds.Piles[@event.ToPileId];
        if (from.Count < @event.Count)
        {
            throw new BoardException("Insufficient cards to draw.");
        }

        var moved = new List<CardState>(@event.Count);
        for (int i = 0; i < @event.Count; i++)
        {
            // Update pile location when moving
            var cardState = from[i];
            moved.Add(new CardState(cardState.Artifact, @event.ToPileId, cardState.IsFaceUp, cardState.Owner));
        }

        var remaining = new List<CardState>(from.Count - @event.Count);
        for (int i = @event.Count; i < from.Count; i++)
            remaining.Add(from[i]);

        var newTo = new List<CardState>(to.Count + moved.Count);
        newTo.AddRange(to);
        newTo.AddRange(moved);

        var newPiles = new Dictionary<string, IList<CardState>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            if (kv.Key == @event.FromPileId)
                newPiles[kv.Key] = remaining;
            else if (kv.Key == @event.ToPileId)
                newPiles[kv.Key] = newTo;
            else
                newPiles[kv.Key] = new List<CardState>(kv.Value);
        }

        var next = new DeckState(ds.Artifact, newPiles, new Dictionary<string, int>(ds.Supply));
        return state.Next([next]);
    }
}

internal sealed class MoveCardsStateMutator : IStateMutator<MoveCardsEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, MoveCardsEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return state; // conditions should have prevented
        }

        if (!ds.Piles.ContainsKey(@event.FromPileId) || !ds.Piles.ContainsKey(@event.ToPileId))
        {
            throw new BoardException("Unknown pile specified.");
        }

        var from = ds.Piles[@event.FromPileId];
        var to = ds.Piles[@event.ToPileId];

        List<CardState> selected;
        if (@event.Cards is not null)
        {
            // preserve order as provided
            // validate presence
            foreach (var c in @event.Cards)
            {
                if (!from.Any(cs => cs.Artifact.Equals(c)))
                    throw new BoardException("Card not present in source pile.");
            }

            selected = new List<CardState>(@event.Cards.Count);
            foreach (var c in @event.Cards)
            {
                var cardState = from.First(cs => cs.Artifact.Equals(c));
                // Update pile location when moving
                selected.Add(new CardState(cardState.Artifact, @event.ToPileId, cardState.IsFaceUp, cardState.Owner));
            }
        }
        else
        {
            var count = @event.Count.GetValueOrDefault();
            if (count == 0)
                return state; // no-op
            if (from.Count < count)
                throw new BoardException("Insufficient cards in source pile.");
            selected = new List<CardState>(count);
            for (int i = 0; i < count; i++)
            {
                var cardState = from[i];
                selected.Add(new CardState(cardState.Artifact, @event.ToPileId, cardState.IsFaceUp, cardState.Owner));
            }
        }

        // build new piles
        var newFrom = new List<CardState>(from);
        // remove selected by identity; stable removal
        foreach (var c in selected)
        {
            var idx = newFrom.FindIndex(x => x.Artifact.Equals(c.Artifact));
            newFrom.RemoveAt(idx);
        }

        var newTo = new List<CardState>(to);
        newTo.AddRange(selected);

        var newPiles = new Dictionary<string, IList<CardState>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            if (kv.Key == @event.FromPileId)
                newPiles[kv.Key] = newFrom;
            else if (kv.Key == @event.ToPileId)
                newPiles[kv.Key] = newTo;
            else
                newPiles[kv.Key] = new List<CardState>(kv.Value);
        }

        var next = new DeckState(ds.Artifact, newPiles, new Dictionary<string, int>(ds.Supply));
        return state.Next([next]);
    }
}

internal sealed class DiscardCardsStateMutator : IStateMutator<DiscardCardsEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, DiscardCardsEvent @event)
    {
        if (@event.Cards.Count == 0)
        {
            return state;
        }

        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return state; // conditions should have prevented
        }

        if (!ds.Piles.ContainsKey(@event.ToPileId))
        {
            throw new BoardException("Unknown pile specified.");
        }

        // Cards may come from multiple piles; validate presence in any pile
        foreach (var c in @event.Cards)
        {
            var present = false;
            foreach (var p in ds.Piles.Values)
            {
                if (p.Any(cs => cs.Artifact.Equals(c)))
                {
                    present = true;
                    break;
                }
            }

            if (!present)
                throw new BoardException("Card not present in any pile.");
        }

        // remove card from its current pile and append to destination in order
        var newPiles = new Dictionary<string, IList<CardState>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            newPiles[kv.Key] = new List<CardState>(kv.Value);
        }

        foreach (var c in @event.Cards)
        {
            CardState? cardState = null;
            foreach (var kv in newPiles)
            {
                var idx = -1;
                var pile = kv.Value;
                for (var i = 0; i < pile.Count; i++)
                {
                    if (pile[i].Artifact.Equals(c))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                {
                    cardState = pile[idx];
                    pile.RemoveAt(idx);
                    break;
                }
            }

            if (cardState != null)
            {
                // Update pile location when discarding
                newPiles[@event.ToPileId].Add(new CardState(cardState.Artifact, @event.ToPileId, cardState.IsFaceUp, cardState.Owner));
            }
        }

        var next = new DeckState(ds.Artifact, newPiles, new Dictionary<string, int>(ds.Supply));
        return state.Next([next]);
    }
}