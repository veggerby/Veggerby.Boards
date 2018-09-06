using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public interface IGameEventRule
    {
        ConditionResponse Check(GameState gameState, IGameEvent @event);
        GameState HandleEvent(GameState gameState, IGameEvent @event);
    }
}