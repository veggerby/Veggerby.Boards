using System.Linq;
using Veggerby.Boards.Core.Flows.Events;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public static class ConditionExtensions
    {
        public static bool IsCompositeCondition<T>(this IGameEventCondition<T> condition, CompositeMode mode) where T : IGameEvent
        {
            return (condition is CompositeGameEventCondition<T>) && ((CompositeGameEventCondition<T>)condition).CompositeMode == mode;
        }

        public static IGameEventCondition<T> And<T>(this IGameEventCondition<T> condition, IGameEventCondition<T> other) where T : IGameEvent
        {
            return CompositeGameEventCondition<T>.CreateCompositeCondition(CompositeMode.All, condition, other);
        }

        public static IGameEventCondition<T> Or<T>(this IGameEventCondition<T> condition, IGameEventCondition<T> other) where T : IGameEvent
        {
            return CompositeGameEventCondition<T>.CreateCompositeCondition(CompositeMode.Any, condition, other);
        }
    }
}