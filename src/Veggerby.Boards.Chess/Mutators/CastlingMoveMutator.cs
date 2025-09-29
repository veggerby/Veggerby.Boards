using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Applies castling: moves king to target (g-file or c-file) and relocates rook; updates castling rights and chess extras bookkeeping.
/// No validation of check/attacked squares (future refinement). Assumes <see cref="CastlingGameEventCondition"/> already validated structure.
/// </summary>
public sealed class CastlingMoveMutator : IStateMutator<MovePieceGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        var extras = gameState.GetExtras<ChessStateExtras>();
        if (extras is null)
        {
            return gameState;
        }

        var ownerId = @event.Piece.Owner.Id;
        var isWhite = ownerId == "white";
        var toId = @event.Path!.To.Id;
        var isKingSide = toId == (isWhite ? "tile-g1" : "tile-g8");
        var isQueenSide = toId == (isWhite ? "tile-c1" : "tile-c8");
        if (!isKingSide && !isQueenSide)
        {
            return gameState; // not a castling move
        }

        var rookSourceId = isWhite
            ? (isKingSide ? "tile-h1" : "tile-a1")
            : (isKingSide ? "tile-h8" : "tile-a8");
        var rookDestId = isWhite
            ? (isKingSide ? "tile-f1" : "tile-d1")
            : (isKingSide ? "tile-f8" : "tile-d8");

        // Resolve rook piece via piece states
        var rookPiece = gameState
            .GetStates<PieceState>()
            .Select(s => s.Artifact)
            .FirstOrDefault(p => p.Id == (isWhite ? (isKingSide ? "white-rook-2" : "white-rook-1") : (isKingSide ? "black-rook-2" : "black-rook-1")));
        if (rookPiece is null)
        {
            return gameState; // inconsistent setup
        }

        var kingState = gameState.GetState<PieceState>(@event.Piece);
        if (!kingState.CurrentTile.Equals(@event.From))
        {
            throw new BoardException("Invalid king from tile during castling");
        }

        // Acquire tiles from engine immutable game
        var kingToTile = engine.Game.GetTile(toId);
        var rookToTile = engine.Game.GetTile(rookDestId);

        var newKingState = new PieceState(@event.Piece, kingToTile);
        var newRookState = new PieceState(rookPiece, rookToTile);

        // Bookkeeping: mark both pieces moved, clear en-passant, increment halfmove (king move resets? per FIDE halfmove clock resets only on pawn move or capture → keep +1)
        var movedSet = extras.MovedPieceIds.ToList();
        if (!movedSet.Contains(@event.Piece.Id)) { movedSet.Add(@event.Piece.Id); }
        if (!movedSet.Contains(rookPiece.Id)) { movedSet.Add(rookPiece.Id); }

        var fullmove = extras.FullmoveNumber + (ownerId == "black" ? 1 : 0);
        var newExtras = extras with
        {
            EnPassantTargetTileId = null,
            HalfmoveClock = extras.HalfmoveClock + 1,
            FullmoveNumber = fullmove,
            MovedPieceIds = movedSet.ToArray(),
            WhiteCanCastleKingSide = isWhite ? false : extras.WhiteCanCastleKingSide,
            WhiteCanCastleQueenSide = isWhite ? false : extras.WhiteCanCastleQueenSide,
            BlackCanCastleKingSide = isWhite ? extras.BlackCanCastleKingSide : false,
            BlackCanCastleQueenSide = isWhite ? extras.BlackCanCastleQueenSide : false
        };

        return gameState.Next([newKingState, newRookState]).ReplaceExtras(newExtras);
    }
}