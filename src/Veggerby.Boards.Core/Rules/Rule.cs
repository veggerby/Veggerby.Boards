using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class Rule<T> : IRule where T : IGameEvent
    {
        public abstract GameState GetState(GameEngine gameEngine, GameState currentState, T @event);

        GameState IRule.GetState(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            if (!(@event is T))
            {
                return null;
            }

            return GetState(gameEngine, currentState, (T)@event);
        }
    }
}