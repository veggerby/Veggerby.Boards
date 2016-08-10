using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class NoMoreRegularDieEndTurnRule : EndTurnRule
    {
        public override RuleCheckState Check(Game game, GameState currentState, IGameEvent @event)
        {
            var dieStates = currentState.GetStates<RegularDie>();

            return !(dieStates?.Any() ?? false) ? RuleCheckState.Valid : RuleCheckState.Fail("MoreDice");
        }
    }
}