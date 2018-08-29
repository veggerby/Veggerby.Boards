using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public class CompositeGameEventRule : IGameEventRule
    {
        public IEnumerable<IGameEventRule> Rules { get; }
        public CompositeMode CompositeMode { get; }

        public CompositeGameEventRule(IEnumerable<IGameEventRule> rules, CompositeMode compositeMode)
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

        public RuleCheckState Check(GameState gameState, IGameEvent @event)
        {
            var results = Rules.Select(x => x.Check(gameState, @event)).ToList();

            var compositionResult = CompositeMode == CompositeMode.All
                ? results.All(x => x.Result == RuleCheckResult.Valid)
                : results.Any(x => x.Result == RuleCheckResult.Valid);

            return compositionResult
                ? RuleCheckState.Valid
                : RuleCheckState.Fail(results.Where(x => x.Result == RuleCheckResult.Invalid));
        }

        public GameState HandleEvent(GameState gameState, IGameEvent @event)
        {
            return Rules.Aggregate(gameState, (state, rule) => rule.HandleEvent(state, @event));
        }
    }
}