using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Moves a single piece from one tile to another, updating the piece collection accordingly.
/// </summary>
public class MovePieceStateMutator : IStateMutator<MovePieceGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);
        var currentState = gameState.GetState<PieceState>(@event.Piece);
        if (currentState is null || currentState.CurrentTile is null || !currentState.CurrentTile.Equals(@event.From))
        {
            throw new BoardException("Invalid from tile on move piece event");
        }

        var newState = new PieceState(@event.Piece, @event.To);
        return gameState.Next([newState]);
    }
}