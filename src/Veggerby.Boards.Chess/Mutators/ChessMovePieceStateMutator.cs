using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Extends normal movement mutator with chess extras bookkeeping (halfmove clock, fullmove number, moved piece ids, en-passant target management).
/// </summary>
public sealed class ChessMovePieceStateMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly MovePieceStateMutator _inner = new();
    private static ChessStateExtras RevokeRights(ChessStateExtras prev, bool whiteKingMoved, bool blackKingMoved,
        bool whiteRookFromA1, bool whiteRookFromH1, bool blackRookFromA8, bool blackRookFromH8,
        bool whiteRookA1Captured = false, bool whiteRookH1Captured = false, bool blackRookA8Captured = false, bool blackRookH8Captured = false,
        bool whiteKingCaptured = false, bool blackKingCaptured = false)
    {
        return prev with
        {
            WhiteCanCastleKingSide = whiteKingMoved || whiteKingCaptured || whiteRookFromH1 || whiteRookH1Captured ? false : prev.WhiteCanCastleKingSide,
            WhiteCanCastleQueenSide = whiteKingMoved || whiteKingCaptured || whiteRookFromA1 || whiteRookA1Captured ? false : prev.WhiteCanCastleQueenSide,
            BlackCanCastleKingSide = blackKingMoved || blackKingCaptured || blackRookFromH8 || blackRookH8Captured ? false : prev.BlackCanCastleKingSide,
            BlackCanCastleQueenSide = blackKingMoved || blackKingCaptured || blackRookFromA8 || blackRookA8Captured ? false : prev.BlackCanCastleQueenSide
        };
    }

    /// <summary>
    /// Applies standard piece movement then updates chess extras (moved set, clocks, en-passant target assignment on double-step).
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        // Capture original from tile before inner mutator (needed for castling rights revocation)
        var originalFromTile = @event.From; // path From prior to mutation
        var updated = _inner.MutateState(engine, gameState, @event);
        var prevExtras = gameState.GetExtras<ChessStateExtras>();
        if (prevExtras is null)
        {
            return updated;
        }

        var moved = prevExtras.MovedPieceIds.Contains(@event.Piece.Id)
            ? prevExtras.MovedPieceIds
            : prevExtras.MovedPieceIds.Concat(new[] { @event.Piece.Id }).ToArray();

        // Reset en-passant by default; set only if this move is a double-step pawn advance (distance == 2)
        string enPassantTarget = null;
        var rolesExtras = gameState.GetExtras<ChessPieceRolesExtras>();
        if (ChessPiece.IsPawn(gameState, @event.Piece.Id) && @event.Distance == 2)
        {
            // Robust intermediate inference (supports either 2 single-step relations or a future potential single relation of distance 2)
            var relations = @event.Path.Relations.ToArray();
            if (relations.Length == 2)
            {
                // Standard case: two explicit single-step relations; intermediate is first To
                enPassantTarget = relations[0].To.Id;
            }
            else if (relations.Length == 1 && relations[0].Distance == 2)
            {
                // Defensive: derive intermediate via coordinate arithmetic (same file, rank +/-2)
                if (ChessCoordinates.TryParse(relations[0].From.Id, out var fFile, out var fRank) && ChessCoordinates.TryParse(relations[0].To.Id, out var tFile, out var tRank) && fFile == tFile && System.Math.Abs(tRank - fRank) == 2)
                {
                    var midRank = (fRank + tRank) / 2; // integer midpoint between ranks (e.g., 2 & 4 -> 3; 7 & 5 -> 6)
                    enPassantTarget = ChessCoordinates.BuildTileId(fFile, midRank);
                }
            }
        }

        var isPawnAdvance = ChessPiece.IsPawn(gameState, @event.Piece.Id);
        var halfmove = isPawnAdvance ? 0 : prevExtras.HalfmoveClock + 1;
        // Derive active player defensively: prefer ActivePlayerState when present, else infer from mover color sequence assumption (white starts)
        string activeId = gameState.TryGetActivePlayer(out var ap)
            ? ap.Id
            : (ChessPiece.IsWhite(gameState, @event.Piece.Id) ? ChessIds.Players.White : ChessIds.Players.Black);
        var fullmove = prevExtras.FullmoveNumber + (activeId == ChessIds.Players.Black ? 1 : 0);

        // Castling rights revocation rules (movement):
        //  * Moving a king removes both rights for that color.
        //  * Moving a rook from its original starting square removes that side's right only.
        bool whiteKingMoved = @event.Piece.Id == ChessIds.Pieces.WhiteKing;
        bool blackKingMoved = @event.Piece.Id == ChessIds.Pieces.BlackKing;
        bool whiteRookFromA1 = @event.Piece.Id == ChessIds.Pieces.WhiteRook1 && originalFromTile?.Id == ChessIds.Tiles.A1;
        bool whiteRookFromH1 = @event.Piece.Id == ChessIds.Pieces.WhiteRook2 && originalFromTile?.Id == ChessIds.Tiles.H1;
        bool blackRookFromA8 = @event.Piece.Id == ChessIds.Pieces.BlackRook1 && originalFromTile?.Id == ChessIds.Tiles.A8;
        bool blackRookFromH8 = @event.Piece.Id == ChessIds.Pieces.BlackRook2 && originalFromTile?.Id == ChessIds.Tiles.H8;

        var rightsAdjusted = RevokeRights(prevExtras, whiteKingMoved, blackKingMoved, whiteRookFromA1, whiteRookFromH1, blackRookFromA8, blackRookFromH8);
        var newExtras = rightsAdjusted with
        {
            EnPassantTargetTileId = enPassantTarget,
            HalfmoveClock = halfmove,
            FullmoveNumber = fullmove,
            MovedPieceIds = moved
        };

        return updated.ReplaceExtras(newExtras);
    }
}