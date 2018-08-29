using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public class CompositeGameEventRule : IGameEventRule<IGameEvent>
    {
        public IEnumerable<IGameEventRule<IGameEvent>> Rules { get; }
        public CompositeMode CompositeMode { get; }

        public CompositeGameEventRule(IEnumerable<IGameEventRule<IGameEvent>> rules, CompositeMode compositeMode)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (!rules.Any())
            {
                throw new ArgumentException("Empty rules list", nameof(rules));
            }

            Rules = rules.ToList();
            CompositeMode = compositeMode;
        }

        private IDictionary<IGameEventRule<IGameEvent>, RuleCheckState> RunCompositeCheck(GameState gameState, IGameEvent @event)
        {
            return Rules.ToDictionary(x => x, x => x.Check(gameState, @event));
        }

        private RuleCheckState GetCompositeRuleCheckState(IDictionary<IGameEventRule<IGameEvent>, RuleCheckState> results)
        {
            var ignoreAll = results.All(x => x.Value.Result == RuleCheckResult.Ignore);

            if (ignoreAll)
            {
                return RuleCheckState.NotApplicable;
            }

            var compositionResult = CompositeMode == CompositeMode.All
                ? results.All(x => x.Value.Result == RuleCheckResult.Valid)
                : results.Any(x => x.Value.Result == RuleCheckResult.Valid);

            return compositionResult
                ? RuleCheckState.Valid
                : RuleCheckState.Fail(results.Select(x => x.Value).Where(x => x.Result == RuleCheckResult.Invalid));
        }

        public RuleCheckState Check(GameState gameState, IGameEvent @event)
        {
            var results = RunCompositeCheck(gameState, @event);
            return GetCompositeRuleCheckState(results);
        }

        public GameState HandleEvent(GameState gameState, IGameEvent @event)
        {
            var results = RunCompositeCheck(gameState, @event);
            var compositeResult = GetCompositeRuleCheckState(results);

            if (compositeResult.Result == RuleCheckResult.Valid)
            {
                // if mode is "any" only mutate state from the FIRST valid rule (top-down)

                if (CompositeMode == CompositeMode.Any)
                {
                    return results
                        .First(x => x.Value.Result == RuleCheckResult.Valid)
                        .Key
                        .HandleEvent(gameState, @event);
                }

                // mode is "all" and all rules are valid, chain state mutators in order

                return results
                    .Aggregate(gameState, (state, rule) => rule.Key.HandleEvent(state, @event));
            }
            else if (compositeResult.Result == RuleCheckResult.Ignore)
            {
                return gameState;
            }

            throw new BoardException("Invalid game event");
        }
    }
}