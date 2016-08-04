using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class Rule<T> : IRule where T : IGameEvent
    {
        public abstract RuleCheckState Check(Game game, GameState currentState, T @event);
        public abstract GameState Evaluate(Game game, GameState currentState, T @event);

        RuleCheckState IRule.Check(Game game, GameState currentState, IGameEvent @event)
        {
            if (!(@event is T))
            {
                return RuleCheckState.NotApplicable;
            }

            return Check(game, currentState, (T)@event);
        }

        GameState IRule.Evaluate(Game game, GameState currentState, IGameEvent @event)
        {
            if (!(@event is T))
            {
                return currentState;
            }

            return Evaluate(game, currentState, (T)@event);
        }
    }
}