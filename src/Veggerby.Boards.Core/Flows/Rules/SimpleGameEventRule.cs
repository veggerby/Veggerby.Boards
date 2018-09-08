using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public class SimpleGameEventRule<T> : GameEventRule<T> where T : IGameEvent
    {
        private readonly IGameEventCondition<T> _condition;

        private SimpleGameEventRule(IGameEventCondition<T> condition, IStateMutator<T> onBeforeEvent, IStateMutator<T> onAfterEvent)
            : base(onBeforeEvent, onAfterEvent)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            _condition = condition;
        }

        protected override ConditionResponse Check(GameState gameState, T @event)
        {
            return _condition.Evaluate(gameState, @event);
        }

        public static IGameEventRule New(IGameEventCondition<T> condition, IStateMutator<T> onBeforeEvent = null, IStateMutator<T> onAfterEvent = null)
        {
            return new SimpleGameEventRule<T>(condition, onBeforeEvent, onAfterEvent ?? new NullStateMutator<T>());
        }
    }
}