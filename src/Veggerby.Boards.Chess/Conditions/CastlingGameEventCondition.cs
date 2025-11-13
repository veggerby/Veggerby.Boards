using System;
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
        ArgumentNullException.ThrowIfNull(engine, nameof(engine));
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(@event, nameof(@event));

        // Resolve role via metadata (no string heuristics)
        if (@event.Piece is null || !ChessPiece.IsKing(state, @event.Piece.Id))
        {
            return ConditionResponse.Ignore("Not a king");
        }

        var extras = state.GetExtras<ChessStateExtras>();
        if (extras is null)
        {
            return ConditionResponse.Ignore("Missing extras");
        }
        var from = @event.Path?.From;
        var to = @event.Path?.To;
        if (from is null || to is null)
        {
            return ConditionResponse.Ignore("Missing path endpoints");
        }

        // Determine color & initial king square baseline (standard chess: king on e-file)
        var owner = @event.Piece.Owner;
        if (owner is null)
        {
            return ConditionResponse.Ignore("Piece owner missing");
        }
        var isWhite = owner.Id == ChessIds.Players.White;
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

        // Path emptiness between king and rook (excluding endpoints). Use explicit constant sequences instead of constructing ids.
        var rankExpected = isWhite ? '1' : '8';
        if (from.Id[^1] != rankExpected || to.Id[^1] != rankExpected)
        {
            return ConditionResponse.Fail("Rank mismatch");
        }

        // Determine intermediate squares the king traverses (which must be empty) and squares between king and rook (for blockage check).
        // For kingside (e -> g): king passes through f; rook path check includes f and g (destination) but g emptiness checked separately below.
        // For queenside (e -> c): king passes through d; additionally squares between king and rook on queenside are d, c, b (excluding a rook square, c destination checked below).
        string[] pathCheckTiles;
        if (isKingSide)
        {
            pathCheckTiles = isWhite
                ? new[] { ChessIds.Tiles.F1 } // only f1 between e1 and g1 (exclude rook h1)
                : new[] { ChessIds.Tiles.F8 };
        }
        else // queen side
        {
            pathCheckTiles = isWhite
                ? new[] { ChessIds.Tiles.D1, ChessIds.Tiles.C1, ChessIds.Tiles.B1 } // exclude rook a1
                : new[] { ChessIds.Tiles.D8, ChessIds.Tiles.C8, ChessIds.Tiles.B8 };
        }

        foreach (var tileConst in pathCheckTiles)
        {
            // Skip destination here; it will be validated for emptiness after loop.
            if (tileConst == to.Id)
            {
                continue;
            }
            var tile = engine.Game.GetTile(tileConst);
            if (tile is null)
            {
                return ConditionResponse.Fail("Board missing tile");
            }
            if (state.GetPiecesOnTile(tile).Any())
            {
                return ConditionResponse.Fail("Path blocked");
            }
        }

        // Destination must be empty for castling (king cannot capture during castling)
        if (to is null || state.GetPiecesOnTile(to).Any())
        {
            return ConditionResponse.Fail("Destination occupied");
        }

        return ConditionResponse.Valid;
    }
}