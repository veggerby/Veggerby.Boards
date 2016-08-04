using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public interface IRule
    {
        RuleCheckState Check(Game game, GameState currentState, IGameEvent @event);
        GameState Evaluate(Game game, GameState currentState, IGameEvent @event);
    }
}