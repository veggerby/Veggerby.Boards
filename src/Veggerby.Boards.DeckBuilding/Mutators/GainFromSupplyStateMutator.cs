using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Mutator applying a gain-from-supply by decrementing the supply and appending the card to the target pile.
/// </summary>
public sealed class GainFromSupplyStateMutator : IStateMutator<GainFromSupplyEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, GainFromSupplyEvent @event)
    {
        var deckState = gameState.GetState<DeckState>(@event.Deck);
        if (deckState is null)
        {
            return gameState;
        }

        // Build new piles dictionary with target pile appended by a Card with matching id.
        var newPiles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in deckState.Piles)
        {
            newPiles[kv.Key] = new List<Card>(kv.Value);
        }

        // Resolve card artifact by id from game artifacts.
        var card = engine.Game.GetArtifact<Card>(@event.CardId);
        if (card is null)
        {
            throw new BoardException($"Card '{@event.CardId}' not found in game artifacts.");
        }

        newPiles[@event.TargetPileId].Add(card);

        // Decrement supply count
        var newSupply = new Dictionary<string, int>(deckState.Supply, StringComparer.Ordinal);
        newSupply[@event.CardId] = newSupply[@event.CardId] - 1;

        var updatedDeckState = new DeckState(@event.Deck, newPiles, newSupply);
        return gameState.Next([updatedDeckState]);
    }
}