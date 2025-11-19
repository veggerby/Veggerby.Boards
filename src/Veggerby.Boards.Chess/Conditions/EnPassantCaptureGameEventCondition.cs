using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Valid when a pawn moves diagonally one square onto an empty tile that matches the en-passant target; the captured pawn is located adjacent.
/// </summary>
public sealed class EnPassantCaptureGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Evaluates whether a pawn move qualifies as an en-passant capture opportunity (structural target match only).
    /// </summary>
    /// <param name="engine">Game engine.</param>
    /// <param name="state">Current state.</param>
    /// <param name="moveEvent">Move event.</param>
    /// <returns>Valid if en-passant target matches diagonal pawn move; otherwise Ignore.</returns>
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent moveEvent)
    {
        ArgumentNullException.ThrowIfNull(engine, nameof(engine));
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(moveEvent, nameof(moveEvent));

        var @event = moveEvent;
        if (!ChessPiece.IsPawn(engine.Game, @event.Piece.Id))
        {
            return ConditionResponse.Ignore("Not a pawn");
        }

        if (@event.Distance != 1)
        {
            return ConditionResponse.Ignore("Not a single step");
        }

        // Must be diagonal (directions count 1 and not straight north/south)
        var dir = @event.Path.Directions.Single();
        if (dir.Id is Constants.Directions.North or Constants.Directions.South)
        {
            return ConditionResponse.Ignore("Not diagonal");
        }

        var extras = state.GetExtras<ChessStateExtras>();
        if (extras?.EnPassantTargetTileId is null)
        {
            return ConditionResponse.Ignore("No en-passant target");
        }

        if (@event.To.Id != extras.EnPassantTargetTileId)
        {
            return ConditionResponse.Ignore("Destination not en-passant target");
        }

        // Destination must be empty (normal capture condition would have triggered otherwise). If any piece present -> ignore.
        if (@event.To is null || state.GetPiecesOnTile(@event.To).Any())
        {
            return ConditionResponse.Ignore("Destination occupied (not en-passant)");
        }

        // Validate structural victim presence: victim is pawn on same file as destination but one rank behind relative to mover direction.
        if (@event.To is null || !ChessCoordinates.TryParse(@event.To.Id, out var file, out var rank))
        {
            return ConditionResponse.Ignore("Unparsable target id");
        }

        var moverIsWhite = ChessPiece.IsWhite(engine.Game, @event.Piece.Id);
        // White moves north; victim pawn is one rank south of target (rank - 1). Black moves south; victim one rank north (rank + 1).
        var victimRank = moverIsWhite ? rank - 1 : rank + 1;
        if (victimRank < 1 || victimRank > 8)
        {
            return ConditionResponse.Ignore("Victim rank outside board");
        }

        var victimTileId = ChessCoordinates.BuildTileId(file, victimRank);
        var victimTile = engine.Game.Board.GetTile(victimTileId);
        var victimPawn = victimTile is null ? null : state.GetPiecesOnTile(victimTile)
            .FirstOrDefault(p => p.Owner is not null && !p.Owner.Equals(@event.Piece.Owner) && ChessPiece.IsPawn(engine.Game, p.Id));

        if (victimPawn is null)
        {
            return ConditionResponse.Ignore("No victim pawn");
        }

        return ConditionResponse.Valid;
    }
}