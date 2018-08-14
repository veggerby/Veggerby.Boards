using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public abstract class GameEventRule : IGameEventRule
    {
        public static readonly IGameEventRule Null = new NullGameEventRule();
        public abstract RuleCheckState Check(GameEngine gameEngine, IGameEvent @event);
    }
}