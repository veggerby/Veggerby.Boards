namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Extension helpers for composing <see cref="IGameStateCondition"/> instances into higher order logical conditions.
/// </summary>
/// <remarks>
/// These helpers provide a fluent API for building composite condition trees without manually instantiating
/// <see cref="CompositeGameStateCondition"/>. All composition preserves immutability – a new composite wrapper is
/// allocated and the original conditions remain unchanged.
/// </remarks>
public static class ConditionExtensions
{
    /// <summary>
    /// Determines whether the condition is a composite condition with the specified <paramref name="mode"/>.
    /// </summary>
    /// <param name="condition">The condition instance to test.</param>
    /// <param name="mode">The composite mode to match.</param>
    /// <returns><c>true</c> if the condition is a composite with the given mode; otherwise <c>false</c>.</returns>
    public static bool IsCompositeCondition(this IGameStateCondition condition, CompositeMode mode)
    {
        return (condition is CompositeGameStateCondition) && ((CompositeGameStateCondition)condition).CompositeMode == mode;
    }

    /// <summary>
    /// Creates a composite condition that is only valid when <b>both</b> the current condition and <paramref name="other"/> are valid.
    /// </summary>
    /// <param name="condition">The left condition.</param>
    /// <param name="other">The right condition to combine.</param>
    /// <returns>A composite ALL condition (logical AND).</returns>
    public static IGameStateCondition And(this IGameStateCondition condition, IGameStateCondition other)
    {
        return CompositeGameStateCondition.CreateCompositeCondition(CompositeMode.All, condition, other);
    }

    /// <summary>
    /// Creates a composite condition that is valid when <b>either</b> the current condition or <paramref name="other"/> is valid.
    /// </summary>
    /// <param name="condition">The left condition.</param>
    /// <param name="other">The right condition to combine.</param>
    /// <returns>A composite ANY condition (logical OR).</returns>
    public static IGameStateCondition Or(this IGameStateCondition condition, IGameStateCondition other)
    {
        return CompositeGameStateCondition.CreateCompositeCondition(CompositeMode.Any, condition, other);
    }
}