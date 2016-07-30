using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class Rule<T> : IRule where T : IGameEvent
    {
        public abstract bool Check(GameEngine gameEngine, GameState currentState, T @event);
        public abstract GameState Evaluate(GameEngine gameEngine, GameState currentState, T @event);

        bool IRule.Check(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            if (!(@event is T))
            {
                return false;
            }

            return Check(gameEngine, currentState, (T)@event);
        }

        GameState IRule.Evaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event)
        {
            if (!(@event is T))
            {
                return currentState;
            }

            return Evaluate(gameEngine, currentState, (T)@event);
        }
    }
}