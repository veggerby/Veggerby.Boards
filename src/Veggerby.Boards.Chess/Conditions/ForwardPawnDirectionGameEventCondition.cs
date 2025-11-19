using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Ensures a pawn single-step move is strictly forward (no diagonal). White forward = north, black forward = south.
/// </summary>
public sealed class ForwardPawnDirectionGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Valid only when a pawn moves exactly one relation in its forward direction (no diagonals or multi-step paths).
    /// </summary>
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
        if (isWhite && dir.Id == Constants.Directions.North)
        {
            return ConditionResponse.Valid;
        }

        if (!isWhite && dir.Id == Constants.Directions.South)
        {
            return ConditionResponse.Valid;
        }

        return ConditionResponse.Ignore("Not forward direction");
    }
}