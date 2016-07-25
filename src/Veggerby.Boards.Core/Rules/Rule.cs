using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class Rule<T> : IRule where T : IGameEvent
    {
        public abstract GameState GetState(GameState currentState, T @event);

        GameState IRule.GetState(GameState currentState, IGameEvent @event)
        {
            if (!(@event is T))
            {
                return null;
            }

            return GetState(currentState, (T)@event);
        }
    }
}