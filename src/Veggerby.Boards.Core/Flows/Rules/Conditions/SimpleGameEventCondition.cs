using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class SimpleGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
    {
        private readonly Func<GameEngine, GameState, T, ConditionResponse> _handler;

        public SimpleGameEventCondition(Func<GameEngine, GameState, T, ConditionResponse> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handler = handler;
        }

        public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
        {
            return _handler(engine, state, @event);
        }
    }
}