using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Appends a <see cref="GameEndedState"/> marker when an <see cref="EndGameEvent"/> is processed.
/// </summary>
public sealed class EndGameStateMutator : IStateMutator<EndGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, EndGameEvent @event)
    {
        // Idempotent: if already ended, no change
        foreach (var _ in state.GetStates<GameEndedState>()) { return state; }
        return state.Next([new GameEndedState()]);
    }
}