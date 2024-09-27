namespace Veggerby.Boards.Core.Flows.Rules;

public static class RuleExtensions
{
    public static bool IsCompositeRule(this IGameEventRule rule, CompositeMode mode)
    {
        return (rule is CompositeGameEventRule) && ((CompositeGameEventRule)rule).CompositeMode == mode;
    }

    public static IGameEventRule And(this IGameEventRule rule1, IGameEventRule rule2)
    {
        return CompositeGameEventRule.CreateCompositeRule(CompositeMode.All, rule1, rule2);
    }

    public static IGameEventRule Or(this IGameEventRule rule1, IGameEventRule rule2)
    {
        return CompositeGameEventRule.CreateCompositeRule(CompositeMode.Any, rule1, rule2);
    }
}