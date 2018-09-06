using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
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

        [Obsolete("Use overload using IGameEventCondition instead")]
        public static IGameEventRule New(Func<GameState, T, ConditionResponse> handler, IStateMutator<T> onBeforeEvent = null, IStateMutator<T> onAfterEvent = null)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return SimpleGameEventRule<T>.New(new SimpleGameEventCondition<T>(handler), onBeforeEvent, onAfterEvent);
        }

        public static IGameEventRule New(IGameEventCondition<T> condition, IStateMutator<T> onBeforeEvent = null, IStateMutator<T> onAfterEvent = null)
        {
            return new SimpleGameEventRule<T>(condition, onBeforeEvent, onAfterEvent ?? new NullStateMutator<T>());
        }
    }
}