using System;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class ConditionalRule : IRule
    {
        private readonly IRule _innerRule;

        public ConditionalRule(IRule innerRule)
        {
            if (innerRule == null)
            {
                throw new ArgumentNullException(nameof(innerRule));
            }

            _innerRule = innerRule;
        }
        
        protected abstract bool EvaluateCondition(GameEngine gameEngine, GameState currentState, IGameEvent @event);

        public bool Check(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            return EvaluateCondition(gameEngine, currentState, @event) && _innerRule.Check(gameEngine, currentState, @event);
        }

        public GameState Evaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            return _innerRule.Evaluate(gameEngine, currentState, @event);
        }
    }
}