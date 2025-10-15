using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Captures exactly one opponent piece on the destination tile (if present) and moves the attacking piece onto it.
/// </summary>
/// <remarks>
/// Currently assumes at most one opponent piece per tile (true for chess). If no opponent piece is found, this mutator returns original state.
/// </remarks>
public sealed class CapturePieceStateMutator : IStateMutator<MovePieceGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        var opponentPiece = gameState
            .GetPiecesOnTile(@event.To)
            .FirstOrDefault(p => p.Owner is not null && !p.Owner.Equals(@event.Piece.Owner));

        if (opponentPiece is null)
        {
            return gameState; // nothing to capture
        }

        var attackerCurrent = gameState.GetState<PieceState>(@event.Piece);
        if (attackerCurrent is null || attackerCurrent.CurrentTile is null || !attackerCurrent.CurrentTile.Equals(@event.From))
        {
            throw new BoardException("Invalid from tile on capture move event");
        }

        // Relocate attacker onto destination and replace opponent state with a captured marker state.
        var attackerNewState = new PieceState(@event.Piece, @event.To);
        var capturedNewState = new CapturedPieceState(opponentPiece);
        return gameState.Next([attackerNewState, capturedNewState]);
    }
}