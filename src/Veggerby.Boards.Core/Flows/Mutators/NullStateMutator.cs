using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

public class NullStateMutator<T> : IStateMutator<T> where T : IGameEvent
{
    public GameState MutateState(GameEngine engine, GameState gameState, T @event)
    {
        return gameState;
    }
}