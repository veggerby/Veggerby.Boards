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

        protected abstract bool EvaluateCondition(GameState currentState, IGameEvent @event);

        public GameState GetState(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            if (EvaluateCondition(currentState, @event))
            {
                return _innerRule.GetState(gameEngine, currentState, @event);
            }

            return null;
        }
    }
}