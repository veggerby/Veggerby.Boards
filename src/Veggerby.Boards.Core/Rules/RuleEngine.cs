using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class RuleEngine
    {
        private readonly IEnumerable<IRule> _rules;

        public RuleEngine(IEnumerable<IRule> rules)
        {
            _rules = (rules ?? Enumerable.Empty<IRule>()).ToList();
        }

        public GameState GetState(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            var newState = currentState;

            foreach (var rule in _rules)
            {
                newState = rule.GetState(gameEngine, newState, @event) ?? newState;
            }

            return newState != currentState ? newState : null;
        }
    }
}