namespace Veggerby.Boards.Core.States
{
    public interface IGameStateCondition
    {
        ConditionResponse Evaluate(GameState state);
    }
}