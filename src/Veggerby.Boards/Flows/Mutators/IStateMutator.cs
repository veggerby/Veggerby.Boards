using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Produces a new <see cref="GameState"/> in response to an event (immutably).
/// </summary>
/// <typeparam name="T">Event type.</typeparam>
public interface IStateMutator<in T> where T : IGameEvent
{
    /// <summary>
    /// Applies the mutator returning a (possibly unchanged) next state.
    /// </summary>
    GameState MutateState(GameEngine engine, GameState gameState, T @event);
}