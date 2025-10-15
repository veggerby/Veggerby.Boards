using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Wraps capture mutator adding chess extras bookkeeping (reset halfmove, clear en-passant, advance fullmove, mark mover).
/// </summary>
public sealed class ChessCapturePieceStateMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly CapturePieceStateMutator _inner = new();
    // Reuse helper from move mutator via local copy (internal duplication avoided by future internal shared helper if needed)
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
    /// Performs capture then updates chess extras (reset halfmove clock, clear en-passant target, advance fullmove if black to move just played, record mover).
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        var updated = _inner.MutateState(engine, gameState, @event);
        var prevExtras = gameState.GetExtras<ChessStateExtras>();
        if (prevExtras is null)
        {
            return updated;
        }

        var moved = prevExtras.MovedPieceIds.Contains(@event.Piece.Id)
            ? prevExtras.MovedPieceIds
            : prevExtras.MovedPieceIds.Concat(new[] { @event.Piece.Id }).ToArray();

        string activeId = gameState.TryGetActivePlayer(out var ap) && ap is not null
            ? ap.Id
            : (ChessPiece.IsWhite(gameState, @event.Piece.Id) ? ChessIds.Players.White : ChessIds.Players.Black);
        var fullmove = prevExtras.FullmoveNumber + (activeId == ChessIds.Players.Black ? 1 : 0);
        // Determine captured piece (present in updated state as a CapturedPieceState newly added by inner mutator)
        // We inspect difference between previous and updated piece states to infer captured artifact & its last tile.
        var previousPieceStates = gameState.GetStates<PieceState>().ToDictionary(s => s.Artifact.Id);
        var updatedPieceStates = updated.GetStates<PieceState>().ToDictionary(s => s.Artifact.Id);
    string? capturedPieceId = null;
        foreach (var kvp in previousPieceStates)
        {
            if (!updatedPieceStates.ContainsKey(kvp.Key))
            {
                capturedPieceId = kvp.Key;
                break;
            }
        }

        bool whiteKingCaptured = capturedPieceId == ChessIds.Pieces.WhiteKing; // extreme edge, typically game over
        bool blackKingCaptured = capturedPieceId == ChessIds.Pieces.BlackKing;
        bool whiteRookA1Captured = capturedPieceId == ChessIds.Pieces.WhiteRook1; // only original squares matter; if rook moved then rights already revoked
        bool whiteRookH1Captured = capturedPieceId == ChessIds.Pieces.WhiteRook2;
        bool blackRookA8Captured = capturedPieceId == ChessIds.Pieces.BlackRook1;
        bool blackRookH8Captured = capturedPieceId == ChessIds.Pieces.BlackRook2;

        var rightsAdjusted = RevokeRights(prevExtras, false, false, false, false, false, false,
            whiteRookA1Captured, whiteRookH1Captured, blackRookA8Captured, blackRookH8Captured, whiteKingCaptured, blackKingCaptured);
        var newExtras = rightsAdjusted with
        {
            EnPassantTargetTileId = null,
            HalfmoveClock = 0,
            FullmoveNumber = fullmove,
            MovedPieceIds = moved
        };
        return updated.ReplaceExtras(newExtras);
    }
}