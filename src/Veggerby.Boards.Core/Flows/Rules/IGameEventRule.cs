using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public interface IGameEventRule
    {
        ConditionResponse Check(GameEngine engine, GameState gameState, IGameEvent @event);
        GameState HandleEvent(GameEngine engine, GameState gameState, IGameEvent @event);
    }
}