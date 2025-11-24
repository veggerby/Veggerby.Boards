using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.Conditions;

/// <summary>
/// Valid when a pawn move is a single-step diagonal forward (relative to its color).
/// Forward for white is south; for black is north. Diagonals are south-east/south-west or north-east/north-west respectively.
/// </summary>
public sealed class DiagonalPawnDirectionGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Validates that the pawn move is a single-step diagonal forward relative to pawn color (white: south-east/south-west, black: north-east/north-west).
    /// </summary>
    /// <param name="engine">Game engine (unused).</param>
    /// <param name="state">Current state (unused).</param>
    /// <param name="moveEvent">Move piece event.</param>
    /// <returns>Valid if pawn forward diagonal single-step, else Ignore.</returns>
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent moveEvent)
    {
        ArgumentNullException.ThrowIfNull(engine, nameof(engine));
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(moveEvent, nameof(moveEvent));

        if (!ChessPiece.IsPawn(engine.Game, moveEvent.Piece.Id))
        {
            return ConditionResponse.Ignore("Not a pawn");
        }

        if (moveEvent.Distance != 1)
        {
            return ConditionResponse.Ignore("Not single step");
        }

        var dir = moveEvent.Path.Directions.Single();
        var isWhite = ChessPiece.IsWhite(engine.Game, moveEvent.Piece.Id);
        if (isWhite)
        {
            if (dir.Id is Veggerby.Boards.Constants.Directions.NorthEast or Veggerby.Boards.Constants.Directions.NorthWest)
            {
                return ConditionResponse.Valid;
            }
        }
        else
        {
            if (dir.Id is Veggerby.Boards.Constants.Directions.SouthEast or Veggerby.Boards.Constants.Directions.SouthWest)
            {
                return ConditionResponse.Valid;
            }
        }

        return ConditionResponse.Ignore("Not forward diagonal for pawn");
    }
}