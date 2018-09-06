using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public class SimpleGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
    {
        private readonly Func<GameState, T, ConditionResponse> _handler;

        public SimpleGameEventCondition(Func<GameState, T, ConditionResponse> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handler = handler;
        }

        public ConditionResponse Evaluate(GameState state, T @event)
        {
            return _handler(state, @event);
        }
    }
}