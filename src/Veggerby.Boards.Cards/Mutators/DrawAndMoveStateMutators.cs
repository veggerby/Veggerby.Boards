using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.States;
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
        var moved = new List<Card>(@event.Count);
        for (int i = 0; i < @event.Count; i++)
        {
            moved.Add(from[i]);
        }
        var remaining = new List<Card>(from.Count - @event.Count);
        for (int i = @event.Count; i < from.Count; i++)
            remaining.Add(from[i]);

        var newTo = new List<Card>(to.Count + moved.Count);
        newTo.AddRange(to);
        newTo.AddRange(moved);

        var newPiles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            if (kv.Key == @event.FromPileId)
                newPiles[kv.Key] = remaining;
            else if (kv.Key == @event.ToPileId)
                newPiles[kv.Key] = newTo;
            else
                newPiles[kv.Key] = new List<Card>(kv.Value);
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

        List<Card> selected;
        if (@event.Cards is not null)
        {
            // preserve order as provided
            // validate presence
            foreach (var c in @event.Cards)
            {
                if (!from.Contains(c))
                    throw new BoardException("Card not present in source pile.");
            }
            selected = new List<Card>(@event.Cards);
        }
        else
        {
            var count = @event.Count.GetValueOrDefault();
            if (count == 0)
                return state; // no-op
            if (from.Count < count)
                throw new BoardException("Insufficient cards in source pile.");
            selected = new List<Card>(count);
            for (int i = 0; i < count; i++)
                selected.Add(from[i]);
        }

        // build new piles
        var newFrom = new List<Card>(from);
        // remove selected by identity; stable removal
        foreach (var c in selected)
        {
            var idx = newFrom.FindIndex(x => x.Equals(c));
            newFrom.RemoveAt(idx);
        }
        var newTo = new List<Card>(to);
        newTo.AddRange(selected);

        var newPiles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            if (kv.Key == @event.FromPileId)
                newPiles[kv.Key] = newFrom;
            else if (kv.Key == @event.ToPileId)
                newPiles[kv.Key] = newTo;
            else
                newPiles[kv.Key] = new List<Card>(kv.Value);
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
                if (p.Contains(c))
                {
                    present = true;
                    break;
                }
            }
            if (!present)
                throw new BoardException("Card not present in any pile.");
        }
        // remove card from its current pile and append to destination in order
        var newPiles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            newPiles[kv.Key] = new List<Card>(kv.Value);
        }
        foreach (var c in @event.Cards)
        {
            foreach (var kv in newPiles)
            {
                var idx = kv.Value.IndexOf(c);
                if (idx >= 0)
                {
                    kv.Value.RemoveAt(idx);
                    break;
                }
            }
            newPiles[@event.ToPileId].Add(c);
        }
        var next = new DeckState(ds.Artifact, newPiles, new Dictionary<string, int>(ds.Supply));
        return state.Next([next]);
    }
}