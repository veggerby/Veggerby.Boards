using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.DeckBuilding.States;
using Veggerby.Boards.DeckBuilding.Artifacts;
using Veggerby.Boards.Cards.States;
namespace Veggerby.Boards.DeckBuilding.Mutators;

/// <summary>
/// Aggregates victory points across player deck piles using registered card definitions.
/// </summary>
public sealed class ComputeScoresStateMutator : IStateMutator<ComputeScoresEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, ComputeScoresEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        // Prevent double computation (condition should block but guard anyway for purity).
        foreach (var _ in state.GetStates<ScoreState>())
        {
            return state;
        }

        // Build lookup: cardId -> victory points
        var vp = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var def in state.GetStates<CardDefinitionState>())
        {
            vp[def.Artifact.Id] = def.Artifact.VictoryPoints;
        }

        var newStates = new List<IArtifactState>();

        // Establish simple mapping: deck id prefix "p1-" -> player id "P1" etc.
        // Locate deck states first.
        var deckStates = state.GetStates<DeckState>();
        // Build a quick dictionary of deck id -> deck state for lookups.
        var deckById = new Dictionary<string, DeckState>(StringComparer.OrdinalIgnoreCase);
        foreach (var ds in deckStates)
        {
            deckById[ds.Artifact.Id] = ds;
        }
        foreach (var player in engine.Game.Players)
        {
            int total = 0;
            // Expected deck id pattern: lower-case player id prefixed with 'p' (builder uses p1-deck, p2-deck)
            var candidateId = player.Id.ToLowerInvariant() + "-deck"; // P1 -> p1-deck
            if (deckById.TryGetValue(candidateId, out var ds))
            {
                foreach (var pile in ds.Piles.Values)
                {
                    foreach (var card in pile)
                    {
                        if (vp.TryGetValue(card.Id, out var points))
                        {
                            total += points;
                        }
                    }
                }
            }
            newStates.Add(new ScoreState(player, total));
        }
        return state.Next(newStates);
    }
}