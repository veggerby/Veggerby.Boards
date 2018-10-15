using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public interface IGameEventCondition<in T> where T : IGameEvent
    {
        ConditionResponse Evaluate(GameEngine engine, GameState state, T @event);
    }
}