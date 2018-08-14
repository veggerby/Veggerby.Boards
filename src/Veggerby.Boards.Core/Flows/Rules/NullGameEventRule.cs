using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    internal sealed class NullGameEventRule : IGameEventRule
    {
        public RuleCheckState Check(Game game, GameState currentState, IGameEvent @event)
        {
            return RuleCheckState.New(RuleCheckResult.Ignore);
        }

        public GameState HandleEvent(Game game, GameState currentState, IGameEvent @event)
        {
            return currentState;
        }
    }
}