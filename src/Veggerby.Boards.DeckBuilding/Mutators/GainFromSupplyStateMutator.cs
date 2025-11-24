using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.DeckBuilding.States;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
namespace Veggerby.Boards.DeckBuilding.Mutators;

/// <summary>
/// Mutator applying a gain-from-supply by decrementing the supply and appending the card to the target pile.
/// </summary>
public sealed class GainFromSupplyStateMutator : IStateMutator<GainFromSupplyEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, GainFromSupplyEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);
        var deckState = gameState.GetState<DeckState>(@event.Deck);
        if (deckState is null)
        {
            return gameState;
        }

        // Selective cloning strategy:
        // 1. Shallow copy dictionary (new dictionary instance) so prior state's dictionary remains untouched.
        // 2. Clone ONLY the target pile list; reuse all other pile list references (lists must be treated as immutable snapshots elsewhere).
        var originalPiles = deckState.Piles;
        if (!originalPiles.TryGetValue(@event.TargetPileId, out var targetPileOriginal))
        {
            // Defensive: condition should have rejected already.
            return gameState;
        }
        // Build a new dictionary with IList<Card> values expected by DeckState. We reuse existing
        // list instances for non-target piles (they will be cloned again inside DeckState's constructor
        // which currently materializes a read-only copy for each pile). This still avoids the previous
        // double-copy of every pile the mutator used to perform before handing off to DeckState.
        var newPiles = new Dictionary<string, IList<Card>>(originalPiles.Count, StringComparer.Ordinal);
        foreach (var kv in originalPiles)
        {
            if (kv.Key.Equals(@event.TargetPileId, StringComparison.Ordinal))
            {
                continue; // handle target separately
            }

            // Reuse existing read-only collection; DeckState constructor will materialize its own frozen copy.
            newPiles[kv.Key] = (IList<Card>)kv.Value; // safe cast (ReadOnlyCollection<Card> implements IList<Card>)
        }

        // Resolve card artifact by id (condition already validated existence, but defensive guard retained).
        var card = engine.Game.GetArtifact<Card>(@event.CardId) ?? throw new BoardException(ExceptionMessages.CardNotFound(@event.CardId));

        var targetCloned = new List<Card>(targetPileOriginal.Count + 1);
        // Copy existing references (no per-item cloning needed; Card artifacts are immutable).
        foreach (var c in targetPileOriginal)
        {
            targetCloned.Add(c);
        }
        targetCloned.Add(card);
        newPiles[@event.TargetPileId] = targetCloned; // only mutated pile gets a new list we populate

        // Decrement supply count
        var newSupply = new Dictionary<string, int>(deckState.Supply, StringComparer.Ordinal);
        var beforeValue = newSupply[@event.CardId];
        var afterValue = beforeValue - 1;
        newSupply[@event.CardId] = afterValue;

        var updatedDeckState = new DeckState(@event.Deck, newPiles, newSupply); // DeckState will freeze lists
        var next = gameState.Next([updatedDeckState]);
        var stats = gameState.GetExtras<DeckSupplyStats>();
        DeckSupplyStats updatedStats = stats is null ? DeckSupplyStats.From(newSupply) : stats.AfterDecrement(beforeValue, afterValue);
        return next.ReplaceExtras(updatedStats);
    }
}