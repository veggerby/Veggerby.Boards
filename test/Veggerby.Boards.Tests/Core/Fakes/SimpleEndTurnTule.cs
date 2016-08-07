using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class SimpleEndTurnRule : EndTurnRule
    {
        public override RuleCheckState Check(Game game, GameState currentState, IGameEvent @event)
        {
            return RuleCheckState.Valid;
        }
    }
}