using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards; // DeckState

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Allows <see cref="EndGameEvent"/> when a <see cref="ScoreState"/> has been produced (scoring completed) and no prior <see cref="GameEndedState"/> exists.
/// Base MVP includes a minimal fixed turn threshold. Extended logic (optional) permits end based on supply depletion
/// configured via <see cref="DeckBuildingEndTriggerOptions"/> on <see cref="DeckBuildingGameBuilder.WithEndTrigger(DeckBuildingEndTriggerOptions)"/>.
/// Supply depletion semantics (if options present):
/// 1) Threshold: number of distinct empty supply piles >= EmptySupplyPilesThreshold (threshold > 0)
/// 2) Key piles: any key pile card id has supply 0
/// If either configured condition is met the turn threshold check is bypassed (still requires scores).
/// </summary>
public sealed class EndGameEventCondition : IGameEventCondition<EndGameEvent>
{
    private const int MaxTurns = 1; // minimal MVP threshold

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, EndGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        // Already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return ConditionResponse.Ignore("Already ended");
        }
        // Require scores first
        if (!state.GetStates<ScoreState>().Any())
        {
            return ConditionResponse.Ignore("Scores not computed yet");
        }
        // Evaluate custom supply depletion trigger if builder configured options.
        var options = state.GetExtras<DeckBuildingEndTriggerOptions>();
        if (options is not null)
        {
            bool thresholdSatisfied = false;
            bool keyPileSatisfied = false;

            // Gather both player deck states (if present) and unify supply dictionaries (distinct card ids).
            IEnumerable<DeckState> deckStates = state.GetStates<DeckState>();
            if (deckStates is not null)
            {
                // Build a merged view of supply counts (last write wins but they should be consistent across decks for shared supply ids in current model).
                var merged = deckStates.SelectMany(d => d.Supply).GroupBy(kv => kv.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().Value, StringComparer.Ordinal);
                if (options.EmptySupplyPilesThreshold > 0)
                {
                    // Fast path: use stats if available.
                    var stats = state.GetExtras<DeckSupplyStats>();
                    if (stats is not null)
                    {
                        thresholdSatisfied = stats.EmptyPiles >= options.EmptySupplyPilesThreshold;
                    }
                    else
                    {
                        var emptyCount = 0;
                        foreach (var kv in merged)
                        {
                            if (kv.Value <= 0)
                            {
                                emptyCount++;
                                if (emptyCount >= options.EmptySupplyPilesThreshold)
                                {
                                    thresholdSatisfied = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (options.KeyPileCardIds.Count > 0)
                {
                    foreach (var id in options.KeyPileCardIds)
                    {
                        if (merged.TryGetValue(id, out var count) && count <= 0)
                        {
                            keyPileSatisfied = true;
                            break;
                        }
                    }
                }
            }
            if (thresholdSatisfied || keyPileSatisfied)
            {
                return ConditionResponse.Valid;
            }
            // Options present but none satisfied -> do not permit (ignore) regardless of turn count.
            return ConditionResponse.Ignore("End trigger options not satisfied");
        }

        // Fallback: require turn threshold.
        var turn = state.GetStates<TurnState>().FirstOrDefault();
        if (turn is not null && turn.TurnNumber < MaxTurns)
        {
            return ConditionResponse.Ignore("Max turns not reached");
        }
        return ConditionResponse.Valid;
    }
}