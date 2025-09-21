namespace Veggerby.Boards.Flows.Rules;

/// <summary>
/// Extension helpers for composing <see cref="IGameEventRule"/> instances.
/// </summary>
public static class RuleExtensions
{
    /// <summary>
    /// Indicates whether a rule is a composite rule using the specified mode.
    /// </summary>
    public static bool IsCompositeRule(this IGameEventRule rule, CompositeMode mode)
    {
        return (rule is CompositeGameEventRule) && ((CompositeGameEventRule)rule).CompositeMode == mode;
    }

    /// <summary>
    /// Combines two rules into an ALL composite (logical AND semantics ignoring NotApplicable responses).
    /// </summary>
    public static IGameEventRule And(this IGameEventRule rule1, IGameEventRule rule2)
    {
        return CompositeGameEventRule.CreateCompositeRule(CompositeMode.All, rule1, rule2);
    }

    /// <summary>
    /// Combines two rules into an ANY composite (logical OR semantics).
    /// </summary>
    public static IGameEventRule Or(this IGameEventRule rule1, IGameEventRule rule2)
    {
        return CompositeGameEventRule.CreateCompositeRule(CompositeMode.Any, rule1, rule2);
    }
}