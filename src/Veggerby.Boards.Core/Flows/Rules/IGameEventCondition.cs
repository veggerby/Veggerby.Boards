using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public interface IGameEventCondition<in T> where T : IGameEvent
    {
        ConditionResponse Evaluate(GameState state, T @event);
    }
}