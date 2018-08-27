using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public class SimpleGameEventRule : IGameEventRule
    {
        private readonly Func<GameState, IGameEvent, RuleCheckState> _handler;

        public SimpleGameEventRule(Func<GameState, IGameEvent, RuleCheckState> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handler = handler;
        }

        public RuleCheckState Check(GameState gameState, IGameEvent @event)
        {
            return _handler(gameState, @event);
        }

        public GameState HandleEvent(GameState gameState, IGameEvent @event)
        {
            return gameState;
        }
    }
}