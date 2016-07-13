using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core
{
    public abstract class Rule
    {
        public abstract GameState GetState(GameState currentState, IGameEvent @event);
    }
}