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
        private readonly IEnumerable<IGameEventRule> _rules;
        private readonly bool _allRulesMustApply;

        public CompositeGameEventRule(IEnumerable<IGameEventRule> rules, bool allRulesMustApply = true)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (!rules.Any())
            {
                throw new ArgumentException("Empty rules list", nameof(rules));
            }

            _rules = rules.ToList();
            _allRulesMustApply = allRulesMustApply;
        }

        private RuleResult InternalEvaluate(Game game, GameState currentState, IGameEvent @event)
        {
            var result = new Dictionary<IGameEventRule, RuleResult>();
            var state = currentState;

            foreach (var rule in _rules)
            {
                var check = rule.Check(game, state, @event);
                if (RuleCheckState.Valid.Equals(check))
                {
                    // mutate state with rule
                    state = rule.HandleEvent(game, state, @event);
                }

                result.Add(rule, new RuleResult(check, state));

                if (RuleCheckState.Valid.Equals(check) && !_allRulesMustApply)
                {
                    // first valid rule, stop looping (evaluated state is new state)
                    break;
                }
            }

            return new RuleResult(
                (_allRulesMustApply ? result.All(x => !RuleCheckState.Invalid.Equals(x.Value.Check)) : true) && result.Any(x => RuleCheckState.Valid.Equals(x.Value.Check))
                    ? RuleCheckState.Valid
                    : RuleCheckState.Fail(result.Where(x => RuleCheckState.Invalid.Equals(x.Value.Check)).Select(x => x.Value.Check)),
                state);
        }

        public RuleCheckState Check(Game game, GameState currentState, IGameEvent @event)
        {
            return InternalEvaluate(game, currentState, @event).Check;
        }

        public GameState HandleEvent(Game game, GameState currentState, IGameEvent @event)
        {
            return InternalEvaluate(game, currentState, @event).State;
        }
    }
}