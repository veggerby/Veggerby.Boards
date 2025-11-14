using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Cards.Rules;

internal sealed class CreateDeckEventCondition : IGameEventCondition<CreateDeckEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, CreateDeckEvent @event)
    {
        // Validate that all declared deck piles exist in payload
        foreach (var p in @event.Deck.Piles)
        {
            if (!@event.Piles.ContainsKey(p))
            {
                return ConditionResponse.Fail($"Missing pile '{p}'");
            }
        }
        return ConditionResponse.Valid;
    }
}

internal sealed class ShuffleDeckEventCondition : IGameEventCondition<ShuffleDeckEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, ShuffleDeckEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
            return ConditionResponse.Fail("Deck not initialized");
        if (!ds.Piles.ContainsKey(@event.PileId))
            return ConditionResponse.Fail("Unknown pile");
        return ConditionResponse.Valid;
    }
}

internal sealed class DrawCardsEventCondition : IGameEventCondition<DrawCardsEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, DrawCardsEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
            return ConditionResponse.Fail("Deck not initialized");
        if (!ds.Piles.ContainsKey(@event.FromPileId) || !ds.Piles.ContainsKey(@event.ToPileId))
            return ConditionResponse.Fail("Unknown pile");
        if (@event.Count < 0)
            return ConditionResponse.Fail("Negative count");
        if (@event.Count == 0)
            return ConditionResponse.Valid;
        if (ds.Piles[@event.FromPileId].Count < @event.Count)
            return ConditionResponse.Fail("Insufficient cards");
        return ConditionResponse.Valid;
    }
}

internal sealed class MoveCardsEventCondition : IGameEventCondition<MoveCardsEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MoveCardsEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
            return ConditionResponse.Fail("Deck not initialized");
        if (!ds.Piles.ContainsKey(@event.FromPileId) || !ds.Piles.ContainsKey(@event.ToPileId))
            return ConditionResponse.Fail("Unknown pile");
        var from = ds.Piles[@event.FromPileId];
        if (@event.Cards is not null)
        {
            // all must be in from
            if (@event.Cards.Any(c => !from.Contains(c)))
                return ConditionResponse.Fail("Card not in source pile");
            return ConditionResponse.Valid;
        }
        var count = @event.Count.GetValueOrDefault();
        if (count < 0)
            return ConditionResponse.Fail("Negative count");
        if (count == 0)
            return ConditionResponse.Valid;
        return from.Count >= count ? ConditionResponse.Valid : ConditionResponse.Fail("Insufficient cards");
    }
}

internal sealed class DiscardCardsEventCondition : IGameEventCondition<DiscardCardsEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, DiscardCardsEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
            return ConditionResponse.Fail("Deck not initialized");
        if (!ds.Piles.ContainsKey(@event.ToPileId))
            return ConditionResponse.Fail("Unknown pile");
        if (@event.Cards is null || @event.Cards.Count == 0)
            return ConditionResponse.Valid;
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
                return ConditionResponse.Fail("Card not present");
        }
        return ConditionResponse.Valid;
    }
}

internal sealed class PeekCardsEventCondition : IGameEventCondition<PeekCardsEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, PeekCardsEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
            return ConditionResponse.Fail("Deck not initialized");
        if (!ds.Piles.ContainsKey(@event.PileId))
            return ConditionResponse.Fail("Unknown pile");
        if (@event.Count < 0)
            return ConditionResponse.Fail("Negative count");
        if (@event.Count == 0)
            return ConditionResponse.Valid;
        if (ds.Piles[@event.PileId].Count < @event.Count)
            return ConditionResponse.Fail("Insufficient cards");
        return ConditionResponse.Valid;
    }
}

internal sealed class RevealCardsEventCondition : IGameEventCondition<RevealCardsEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, RevealCardsEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
            return ConditionResponse.Fail("Deck not initialized");
        if (!ds.Piles.ContainsKey(@event.PileId))
            return ConditionResponse.Fail("Unknown pile");
        if (@event.Cards is null || @event.Cards.Count == 0)
            return ConditionResponse.Valid;
        var pile = ds.Piles[@event.PileId];
        foreach (var c in @event.Cards)
        {
            if (!pile.Contains(c))
                return ConditionResponse.Fail("Card not in specified pile");
        }
        return ConditionResponse.Valid;
    }
}

internal sealed class ReshuffleEventCondition : IGameEventCondition<ReshuffleEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, ReshuffleEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
            return ConditionResponse.Fail("Deck not initialized");
        if (!ds.Piles.ContainsKey(@event.FromPileId) || !ds.Piles.ContainsKey(@event.ToPileId))
            return ConditionResponse.Fail("Unknown pile");
        return ConditionResponse.Valid;
    }
}