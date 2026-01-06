using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
namespace Veggerby.Boards.Cards.Mutators;

internal sealed class CreateDeckStateMutator : IStateMutator<CreateDeckEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, CreateDeckEvent @event)
    {
        // Replace or add DeckState
        var piles = new Dictionary<string, IList<CardState>>(StringComparer.Ordinal);
        foreach (var kv in @event.Piles)
        {
            var cards = kv.Value?.ToList() ?? new List<Card>();
            var cardStates = new List<CardState>(cards.Count);
            foreach (var card in cards)
            {
                // New cards start face-down (hidden) with no owner
                cardStates.Add(new CardState(card, kv.Key, isFaceUp: false, owner: null));
            }

            piles[kv.Key] = cardStates;
        }

        var ds = new DeckState(@event.Deck, piles, @event.Supply);
        return state.Next([ds]);
    }
}