using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

/// <summary>
/// A no-op mutator that leaves the game state unchanged. Useful as a placeholder or default implementation.
/// </summary>
public class NullStateMutator<T> : IStateMutator<T> where T : IGameEvent
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, T @event)
    {
        return gameState;
    }
}