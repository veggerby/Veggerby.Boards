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
        if (!moveEvent.Piece.Id.Contains("pawn"))
        {
            return ConditionResponse.Ignore("Not a pawn");
        }

        if (moveEvent.Distance != 1)
        {
            return ConditionResponse.Ignore("Not single step");
        }

        var dir = moveEvent.Path.Directions.Single();
        var isWhite = moveEvent.Piece.Id.StartsWith("white-");
        if (isWhite && dir.Id == "north")
        {
            return ConditionResponse.Valid;
        }

        if (!isWhite && dir.Id == "south")
        {
            return ConditionResponse.Valid;
        }

        return ConditionResponse.Ignore("Not forward direction");
    }
}