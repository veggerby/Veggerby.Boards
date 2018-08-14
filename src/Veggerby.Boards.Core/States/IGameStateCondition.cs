namespace Veggerby.Boards.Core.States
{
    public interface IGameStateCondition
    {
        bool Evaluate(GameState state);
    }
}