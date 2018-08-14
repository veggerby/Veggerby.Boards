using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public interface IGameEventRule
    {
        RuleCheckState Check(Game game, GameState currentState, IGameEvent @event);
        GameState HandleEvent(Game game, GameState currentState, IGameEvent @event);
    }
}