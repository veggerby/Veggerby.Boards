using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public interface IRule
    {
        GameState GetState(GameState currentState, IGameEvent @event);
    }
}