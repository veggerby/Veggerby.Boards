namespace Veggerby.Boards.Core.States.Conditions;

public class NullGameStateCondition(bool result) : IGameStateCondition
{
    private readonly bool _result = result;

    public NullGameStateCondition() : this(true) // needed for initializer
    {
    }

    public ConditionResponse Evaluate(GameState state)
    {
        return _result ? ConditionResponse.Valid : ConditionResponse.Invalid;
    }
}