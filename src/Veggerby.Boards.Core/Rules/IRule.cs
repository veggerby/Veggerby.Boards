using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public interface IRule
    {
        bool Check(GameEngine gameEngine, GameState currentState, IGameEvent @event);
        GameState Evaluate(GameEngine gameEngine, GameState currentState, IGameEvent @event);
    }
}