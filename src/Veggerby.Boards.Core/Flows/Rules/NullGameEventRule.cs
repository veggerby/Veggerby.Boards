using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    internal sealed class NullGameEventRule : IGameEventRule
    {
        public RuleCheckState Check(GameEngine gameEngine, IGameEvent @event)
        {
            return RuleCheckState.New(RuleCheckResult.Ignore);
        }
    }
}