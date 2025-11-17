using System;

using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.Mutators;

/// <summary>
/// Marks the game as ended with stalemate outcome when activated.
/// </summary>
/// <remarks>
/// This mutator should be used in a terminal phase (e.g., stalemate phase) to add
/// both GameEndedState and ChessOutcomeState when the phase becomes active.
/// </remarks>
public sealed class StalemateOutcomeMutator : IStateMutator<IGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var outcomeState = new ChessOutcomeState(EndgameStatus.Stalemate, null);
        return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
    }
}
