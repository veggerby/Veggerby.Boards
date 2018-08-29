using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public interface IGameEventRule<in T> where T : IGameEvent
    {
        RuleCheckState Check(GameState gameState, T @event);
        GameState HandleEvent(GameState gameState, T @event);
    }
}