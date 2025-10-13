using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Performs a draw from the draw pile into the hand, reshuffling discard into draw if needed using deterministic RNG.
/// </summary>
public sealed class DrawWithReshuffleStateMutator : IStateMutator<DrawWithReshuffleEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, DrawWithReshuffleEvent @event)
    {
        if (@event.Count == 0)
        {
            return state;
        }

        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return state;
        }

        var drawId = DeckBuildingGameBuilder.Piles.Draw;
        var discardId = DeckBuildingGameBuilder.Piles.Discard;
        var handId = DeckBuildingGameBuilder.Piles.Hand;

        if (!ds.Piles.ContainsKey(drawId) || !ds.Piles.ContainsKey(discardId) || !ds.Piles.ContainsKey(handId))
        {
            throw new BoardException("Required piles missing");
        }

        // Clone piles into mutable lists
        var piles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal);
        foreach (var kv in ds.Piles)
        {
            piles[kv.Key] = new List<Card>(kv.Value);
        }

        var draw = (List<Card>)piles[drawId];
        var discard = (List<Card>)piles[discardId];
        var hand = (List<Card>)piles[handId];

        // If draw is insufficient, shuffle discard deterministically and move onto draw
        var needed = @event.Count - draw.Count;
        if (needed > 0 && discard.Count > 0)
        {
            // Fisher-Yates using state.Random
            var rng = state.Random;
            if (rng is null)
            {
                // No RNG snapshot => do not reorder; move as-is for determinism
            }
            else
            {
                for (int i = discard.Count - 1; i > 0; i--)
                {
                    var u = rng.NextUInt();
                    var j = (int)(u % (uint)(i + 1));
                    (discard[i], discard[j]) = (discard[j], discard[i]);
                }
            }
            // Move entire discard onto draw (top of shuffled discard becomes end of draw)
            draw.AddRange(discard);
            discard.Clear();
        }

        // Now perform the draw
        var toDraw = Math.Min(@event.Count, draw.Count);
        for (int i = 0; i < toDraw; i++)
        {
            hand.Add(draw[i]);
        }
        draw.RemoveRange(0, toDraw);

        var next = new DeckState(ds.Artifact, piles, new Dictionary<string, int>(ds.Supply, StringComparer.Ordinal));
        return state.Next([next]);
    }
}