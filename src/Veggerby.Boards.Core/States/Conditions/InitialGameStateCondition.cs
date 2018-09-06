namespace Veggerby.Boards.Core.States.Conditions
{
    public class InitialGameStateCondition : IGameStateCondition
    {
        public bool Evaluate(GameState state)
        {
            return state.IsInitialState;
        }
    }
}