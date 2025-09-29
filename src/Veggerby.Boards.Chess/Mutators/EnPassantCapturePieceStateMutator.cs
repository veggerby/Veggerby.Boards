using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Performs an en-passant capture: move capturing pawn onto target square and mark victim pawn captured from adjacent square.
/// </summary>
public sealed class EnPassantCapturePieceStateMutator : IStateMutator<MovePieceGameEvent>
{
    /// <summary>
    /// Performs en-passant capture: mover ends on en-passant target tile; victim pawn (adjacent behind target) becomes captured.
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        var extras = gameState.GetExtras<ChessStateExtras>();
        if (extras?.EnPassantTargetTileId is null || extras.EnPassantTargetTileId != @event.To.Id)
        {
            return gameState; // not an en-passant situation
        }

        // Identify victim tile based on mover color (victim is one rank forward from target relative to mover's movement direction).
        if (!ChessCoordinates.TryParse(@event.To.Id, out var file, out var rank))
        {
            return gameState; // invalid target id pattern; defensive no-op
        }

        var moverIsWhite = @event.Piece.Id.StartsWith("white-");
        // White moves north; victim is the pawn that advanced two squares and sits one rank south of the target.
        var victimRank = moverIsWhite ? rank - 1 : rank + 1;

        if (victimRank < 1 || victimRank > 8)
        {
            return gameState; // out of bounds
        }

        var victimTileId = ChessCoordinates.BuildTileId(file, victimRank);
        var victimTile = engine.Game.Board.GetTile(victimTileId);
        var victimPiece = gameState
            .GetPiecesOnTile(victimTile)
            .FirstOrDefault(p => p.Owner is not null && !p.Owner.Equals(@event.Piece.Owner) && p.Id.Contains("pawn"));

        if (victimPiece is null)
        {
            return gameState; // structural mismatch; treat as no-op (condition should have prevented)
        }

        // Move attacker onto en-passant target
        var attackerNewState = new PieceState(@event.Piece, @event.To);
        var capturedNewState = new CapturedPieceState(victimPiece);

        // Reset halfmove, clear en-passant target, update moved set & fullmove if black just moved
        var prevExtras = extras;
        var moved = prevExtras.MovedPieceIds.Contains(@event.Piece.Id)
            ? prevExtras.MovedPieceIds
            : prevExtras.MovedPieceIds.Concat(new[] { @event.Piece.Id }).ToArray();
        string activeId;
        try
        {
            activeId = gameState.GetActivePlayer().Id;
        }
        catch
        {
            activeId = @event.Piece.Id.StartsWith("white-") ? "white" : "black";
        }
        var fullmove = prevExtras.FullmoveNumber + (activeId == "black" ? 1 : 0);
        var newExtras = prevExtras with
        {
            EnPassantTargetTileId = null,
            HalfmoveClock = 0,
            FullmoveNumber = fullmove,
            MovedPieceIds = moved
        };

        var updated = gameState.Next([attackerNewState, capturedNewState]);
        return updated.ReplaceExtras(newExtras);
    }
}