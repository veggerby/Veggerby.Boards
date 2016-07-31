using System;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class ConditionalRule : IRule
    {
        private readonly IRule _ruleIfTrue;
        private readonly IRule _ruleIfFalse;

        public ConditionalRule(IRule ruleIfTrue, IRule ruleIfFalse = null)
        {
            if (ruleIfTrue == null)
            {
                throw new ArgumentNullException(nameof(ruleIfTrue));
            }

            _ruleIfTrue = ruleIfTrue;
            _ruleIfFalse = ruleIfFalse;
        }

        protected abstract bool EvaluateCondition(GameEngine gameEngine, GameState currentState, IGameEvent @event);

        public bool Check(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            var condition = EvaluateCondition(gameEngine, currentState, @event);
            
            return condition ? _ruleIfTrue.Check(gameEngine, currentState, @event) : (_ruleIfFalse?.Check(gameEngine, currentState, @event) ?? false);
        }

        public GameState Evaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            return EvaluateCondition(gameEngine, currentState, @event)
                ? _ruleIfTrue.Evaluate(gameEngine, currentState, @event)
                : (_ruleIfFalse?.Evaluate(gameEngine, currentState, @event) ?? currentState);
        }
    }
}