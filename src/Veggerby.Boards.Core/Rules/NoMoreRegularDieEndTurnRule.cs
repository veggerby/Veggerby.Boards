using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class NoMoreRegularDieEndTurnRule : EndTurnRule
    {
        public override bool IsEndOfTurn(GameState currentState)
        {
            var dieStates = currentState.GetStates<RegularDie>();

            return !(dieStates?.Any() ?? false);
        }
    }
}