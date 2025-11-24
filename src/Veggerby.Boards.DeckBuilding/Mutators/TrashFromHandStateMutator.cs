using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

using Veggerby.Boards.DeckBuilding.Events;
namespace Veggerby.Boards.DeckBuilding.Mutators;

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
            throw new BoardException(ExceptionMessages.HandPileMissing);
        }

        // Optimized cloning: only mutate Hand pile; other piles re-used via existing read-only collections (no reallocation).
        var piles = new Dictionary<string, IList<Card>>(ds.Piles.Count, StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            if (kv.Key.Equals(handId, StringComparison.Ordinal))
            {
                // Create a mutable working list for hand modifications.
                var handList = new List<Card>(kv.Value);
                // remove each specified card by identity
                foreach (var c in @event.Cards)
                {
                    var idx = handList.IndexOf(c);
                    if (idx >= 0)
                    {
                        handList.RemoveAt(idx);
                    }
                }

                piles[kv.Key] = handList;
            }
            else
            {
                // Reuse existing read-only list; constructor will wrap again but avoids intermediate copy here.
                piles[kv.Key] = (IList<Card>)kv.Value; // underlying type is ReadOnlyCollection<Card> implementing IList<Card>
            }
        }

        var next = new DeckState(ds.Artifact, piles, ds.Supply is null ? null : new Dictionary<string, int>(ds.Supply, StringComparer.Ordinal));
        return state.Next([next]);
    }
}