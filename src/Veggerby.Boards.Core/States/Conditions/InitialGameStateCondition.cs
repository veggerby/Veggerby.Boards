namespace Veggerby.Boards.Core.States.Conditions
{
    public class InitialGameStateCondition : IGameStateCondition
    {
        public ConditionResponse Evaluate(GameState state)
        {
            return state.IsInitialState ? ConditionResponse.Valid : ConditionResponse.Invalid;
        }
    }
}