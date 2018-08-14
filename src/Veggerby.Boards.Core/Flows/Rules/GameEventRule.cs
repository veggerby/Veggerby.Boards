using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public abstract class GameEventRule : IGameEventRule
    {
        public static readonly IGameEventRule Null = new NullGameEventRule();
        public abstract RuleCheckState Check(Game game, GameState currentState, IGameEvent @event);
        public abstract GameState HandleEvent(Game game, GameState currentState, IGameEvent @event);
    }
}