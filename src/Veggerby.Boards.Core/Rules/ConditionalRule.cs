using System;
using Veggerby.Boards.Core.Artifacts;
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

        protected abstract bool EvaluateCondition(Game game, GameState currentState, IGameEvent @event);

        public RuleCheckState Check(Game game, GameState currentState, IGameEvent @event)
        {
            var condition = EvaluateCondition(game, currentState, @event);
            
            return condition ? _ruleIfTrue.Check(game, currentState, @event) : (_ruleIfFalse?.Check(game, currentState, @event) ?? RuleCheckState.Invalid);
        }

        public GameState Evaluate(Game game, GameState currentState, IGameEvent @event)
        {
            return EvaluateCondition(game, currentState, @event)
                ? _ruleIfTrue.Evaluate(game, currentState, @event)
                : (_ruleIfFalse?.Evaluate(game, currentState, @event) ?? currentState);
        }
    }
}