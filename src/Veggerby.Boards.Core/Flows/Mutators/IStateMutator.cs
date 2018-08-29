using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public interface IStateMutator<in T> where T : IGameEvent
    {
        GameState MutateState(GameState gameState, T @event);
    }
}