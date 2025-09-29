using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Determines whether a king move attempt is a valid castling invocation.
/// Basic v1: verifies castling rights flags, king on original square (e-file), destination square matches canonical targets (g/c files),
/// path tiles empty between king and rook (excluding rook square), and destination empty.
/// Does NOT (yet) validate check / attacked squares. That refinement can be layered later.
/// Returns Ignore for non-king moves or normal king single-square moves; Valid only for syntactically correct castling path.
/// </summary>
public sealed class CastlingGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Evaluates whether the supplied king movement event constitutes a structurally valid castling attempt.
    /// </summary>
    /// <remarks>
    /// Validation scope (v1):
    ///  * King must be on its original square (d1/d8 per current builder orientation).
    ///  * Destination must be the appropriate castling target file (g or c on same rank).
    ///  * Castling rights flag for the side/direction must still be available.
    ///  * All intermediate squares between king and rook (excluding the rook square) must be empty.
    /// Exclusions (future refinement): does NOT check for king in check, passing through check, or destination under attack.
    /// Non-king moves or ordinary king moves are returned as Ignore to allow other rule branches to process them.
    /// </remarks>
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        // Only consider king pieces by id convention
        if (!@event.Piece.Id.EndsWith(ChessIds.PieceSuffixes.King))
        {
            return ConditionResponse.Ignore("Not a king");
        }

        var extras = state.GetExtras<ChessStateExtras>();
        var from = @event.Path?.From;
        var to = @event.Path?.To;
        if (from is null || to is null)
        {
            return ConditionResponse.Ignore("Missing path endpoints");
        }

        // Determine color & initial king square baseline (standard chess: king on e-file)
        var isWhite = @event.Piece.Owner.Id == ChessIds.Players.White;
        var startTileId = isWhite ? ChessIds.Tiles.E1 : ChessIds.Tiles.E8;
        if (from.Id != startTileId)
        {
            return ConditionResponse.Ignore("King not on initial square");
        }

        // Supported destinations relative to e-file king start:
        // Kingside: e -> g (two squares toward h rook)
        // Queenside: e -> c (two squares toward a rook)
        var kingSideTarget = isWhite ? ChessIds.Tiles.G1 : ChessIds.Tiles.G8;
        var queenSideTarget = isWhite ? ChessIds.Tiles.C1 : ChessIds.Tiles.C8;

        var isKingSide = to.Id == kingSideTarget;
        var isQueenSide = to.Id == queenSideTarget;
        if (!isKingSide && !isQueenSide)
        {
            return ConditionResponse.Ignore("Not a castling target");
        }

        // Rights flags
        if (isKingSide && (isWhite ? !extras.WhiteCanCastleKingSide : !extras.BlackCanCastleKingSide))
        {
            return ConditionResponse.Fail("King-side right not available");
        }
        if (isQueenSide && (isWhite ? !extras.WhiteCanCastleQueenSide : !extras.BlackCanCastleQueenSide))
        {
            return ConditionResponse.Fail("Queen-side right not available");
        }

        // Path emptiness between king and rook (excluding endpoints). Extract all intermediate tiles linearly between from/to by file ordering.
        // Board uses algebraic style ids tile-<file><rank>.
        var fromFile = from.Id[5]; // tile-<f><r>
        var toFile = to.Id[5];
        var rank = from.Id[6]; // same rank for castling
        if (rank != (isWhite ? '1' : '8'))
        {
            return ConditionResponse.Fail("Rank mismatch");
        }
        int step = fromFile < toFile ? 1 : -1;
        for (char f = (char)(fromFile + step); f != toFile; f = (char)(f + step))
        {
            var tileId = $"tile-{f}{rank}";
            var tile = engine.Game.GetTile(tileId);
            // Skip the rook square itself (not part of king traversal). Rook squares: white h1/a1; black h8/a8.
            if (tile.Id == (isWhite ? (isKingSide ? ChessIds.Tiles.H1 : ChessIds.Tiles.A1) : (isKingSide ? ChessIds.Tiles.H8 : ChessIds.Tiles.A8))) { continue; }
            if (state.GetPiecesOnTile(tile).Any()) { return ConditionResponse.Fail("Path blocked"); }
        }

        // Destination must be empty for castling (king cannot capture during castling)
        if (state.GetPiecesOnTile(to).Any())
        {
            return ConditionResponse.Fail("Destination occupied");
        }

        return ConditionResponse.Valid;
    }
}