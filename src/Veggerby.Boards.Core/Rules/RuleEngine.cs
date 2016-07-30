using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class RuleEngine : IRule
    {
        private readonly IEnumerable<IRule> _rules;

        public RuleEngine(IEnumerable<IRule> rules)
        {
            _rules = (rules ?? Enumerable.Empty<IRule>()).ToList();
        }

        private Tuple<bool, GameState> InternalEvaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            bool check = false;
            var state = currentState;

            foreach (var rule in _rules)
            {
                if (rule.Check(gameEngine, state, @event))
                {
                    check = true;
                    state = rule.Evaluate(gameEngine, state, @event);
                }
            }

            return new Tuple<bool, GameState>(check, state);
        }

        public bool Check(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            return InternalEvaluate(gameEngine, currentState, @event).Item1;
        }

        public GameState Evaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            return InternalEvaluate(gameEngine, currentState, @event).Item2;
        }
    }
}