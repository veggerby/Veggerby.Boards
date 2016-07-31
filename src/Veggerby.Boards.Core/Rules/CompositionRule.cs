using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class CompositionRule : IRule
    {
        private readonly IEnumerable<IRule> _rules;
        private readonly bool _allRulesMustApply;

        public CompositionRule(IEnumerable<IRule> rules, bool allRulesMustApply = true)
        {
            _rules = (rules ?? Enumerable.Empty<IRule>()).ToList();
            _allRulesMustApply = allRulesMustApply;
        }

        private RuleResult InternalEvaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            var result = new Dictionary<IRule, RuleResult>();
            var state = currentState;

            foreach (var rule in _rules)
            {
                var check = rule.Check(gameEngine, state, @event);
                if (check)
                {
                    // mutate state with rule
                    state = rule.Evaluate(gameEngine, state, @event);
                }

                result.Add(rule, new RuleResult(check, state));

                if (check && !_allRulesMustApply)
                {
                    // first valid rule, stop looping (evaluated state is new state)
                    break;
                }
            }

            return new RuleResult(
                _allRulesMustApply ? result.All(x => x.Value.Check) : result.Any(x => x.Value.Check), 
                state);
        }

        public bool Check(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            return InternalEvaluate(gameEngine, currentState, @event).Check;
        }

        public GameState Evaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            return InternalEvaluate(gameEngine, currentState, @event).State;
        }
    }
}