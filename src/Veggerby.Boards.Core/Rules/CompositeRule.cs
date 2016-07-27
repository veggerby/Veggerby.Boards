using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class CompositeRule : IRule
    {
        private readonly IEnumerable<IRule> _rules;

        public CompositeRule(IEnumerable<IRule> rules)
        {
            _rules = (rules ?? Enumerable.Empty<IRule>()).ToList();
        }

        public GameState GetState(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            var state = currentState;
            foreach (var rule in _rules)
            {
                state = rule?.GetState(gameEngine, currentState, @event) ?? state;
            }

            if (state == currentState)
            {
                return null;
            }

            return state;
        }
    }
}