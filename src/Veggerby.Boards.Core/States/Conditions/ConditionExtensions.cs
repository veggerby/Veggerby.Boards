namespace Veggerby.Boards.Core.States.Conditions;

public static class ConditionExtensions
{
    public static bool IsCompositeCondition(this IGameStateCondition condition, CompositeMode mode)
    {
        return (condition is CompositeGameStateCondition) && ((CompositeGameStateCondition)condition).CompositeMode == mode;
    }

    public static IGameStateCondition And(this IGameStateCondition condition, IGameStateCondition other)
    {
        return CompositeGameStateCondition.CreateCompositeCondition(CompositeMode.All, condition, other);
    }

    public static IGameStateCondition Or(this IGameStateCondition condition, IGameStateCondition other)
    {
        return CompositeGameStateCondition.CreateCompositeCondition(CompositeMode.Any, condition, other);
    }
}