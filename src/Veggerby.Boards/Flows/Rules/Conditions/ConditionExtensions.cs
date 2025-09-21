using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Composition helpers for <see cref="IGameEventCondition{T}"/>.
/// </summary>
public static class ConditionExtensions
{
    /// <summary>
    /// Determines whether the condition is a composite using the specified mode.
    /// </summary>
    public static bool IsCompositeCondition<T>(this IGameEventCondition<T> condition, CompositeMode mode) where T : IGameEvent
    {
        return (condition is CompositeGameEventCondition<T>) && ((CompositeGameEventCondition<T>)condition).CompositeMode == mode;
    }

    /// <summary>
    /// Logical AND composition (flattens where possible).
    /// </summary>
    public static IGameEventCondition<T> And<T>(this IGameEventCondition<T> condition, IGameEventCondition<T> other) where T : IGameEvent
    {
        return CompositeGameEventCondition<T>.CreateCompositeCondition(CompositeMode.All, condition, other);
    }

    /// <summary>
    /// Logical OR composition (flattens where possible).
    /// </summary>
    public static IGameEventCondition<T> Or<T>(this IGameEventCondition<T> condition, IGameEventCondition<T> other) where T : IGameEvent
    {
        return CompositeGameEventCondition<T>.CreateCompositeCondition(CompositeMode.Any, condition, other);
    }
}