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
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);
        var extras = gameState.GetExtras<ChessStateExtras>();
        if (extras is null)
        {
            return gameState;
        }

        var ownerId = @event.Piece.Owner?.Id;
        if (ownerId is null)
        {
            return gameState;
        }
        var isWhite = ownerId == ChessIds.Players.White;
        var toId = @event.Path?.To?.Id;
        if (toId is null)
        {
            return gameState;
        }
        var isKingSide = toId == (isWhite ? ChessIds.Tiles.G1 : ChessIds.Tiles.G8);
        var isQueenSide = toId == (isWhite ? ChessIds.Tiles.C1 : ChessIds.Tiles.C8);
        if (!isKingSide && !isQueenSide)
        {
            return gameState; // not a castling move
        }

        var rookSourceId = isWhite
            ? (isKingSide ? ChessIds.Tiles.H1 : ChessIds.Tiles.A1)
            : (isKingSide ? ChessIds.Tiles.H8 : ChessIds.Tiles.A8);
        var rookDestId = isWhite
            ? (isKingSide ? ChessIds.Tiles.F1 : ChessIds.Tiles.D1)
            : (isKingSide ? ChessIds.Tiles.F8 : ChessIds.Tiles.D8);

        // Resolve rook piece via piece states
        var rookPiece = gameState
            .GetStates<PieceState>()
            .Select(s => s.Artifact)
            .FirstOrDefault(p => p.Id == (isWhite ? (isKingSide ? ChessIds.Pieces.WhiteRook2 : ChessIds.Pieces.WhiteRook1) : (isKingSide ? ChessIds.Pieces.BlackRook2 : ChessIds.Pieces.BlackRook1)));
        if (rookPiece is null)
        {
            return gameState; // inconsistent setup
        }

        var kingState = gameState.GetState<PieceState>(@event.Piece);
        if (kingState?.CurrentTile is null || !kingState.CurrentTile.Equals(@event.From))
        {
            throw new BoardException("Invalid king from tile during castling");
        }

        // Acquire tiles from engine immutable game
        var kingToTile = engine.Game.GetTile(toId);
        var rookToTile = engine.Game.GetTile(rookDestId);
        if (kingToTile is null || rookToTile is null)
        {
            return gameState;
        }

        var newKingState = new PieceState(@event.Piece, kingToTile);
        var newRookState = new PieceState(rookPiece, rookToTile);

        // Bookkeeping: mark both pieces moved, clear en-passant, increment halfmove (king move resets? per FIDE halfmove clock resets only on pawn move or capture → keep +1)
        var movedSet = extras.MovedPieceIds.ToList();
        if (!movedSet.Contains(@event.Piece.Id)) { movedSet.Add(@event.Piece.Id); }
        if (!movedSet.Contains(rookPiece.Id)) { movedSet.Add(rookPiece.Id); }

        var fullmove = extras.FullmoveNumber + (ownerId == ChessIds.Players.Black ? 1 : 0);
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