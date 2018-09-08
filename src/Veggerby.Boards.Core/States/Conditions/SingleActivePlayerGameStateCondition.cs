using System.Linq;

namespace Veggerby.Boards.Core.States.Conditions
{
    public class SingleActivePlayerGameStateCondition : IGameStateCondition
    {
        public ConditionResponse Evaluate(GameState state)
        {
            return state.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive) != null
                ? ConditionResponse.Valid
                : ConditionResponse.Invalid;
        }
    }
}