namespace Veggerby.Boards.Core.States.Conditions
{
    public class InitialStateGamePhaseCondition : IGameStateCondition
    {
        public bool Evaluate(GameState state)
        {
            return state.IsInitialState;
        }
    }
}