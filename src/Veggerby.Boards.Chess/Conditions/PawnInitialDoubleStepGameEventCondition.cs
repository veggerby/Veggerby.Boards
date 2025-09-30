using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Valid only if the move event represents a pawn attempting its initial two-square advance from the starting rank
/// with both intermediate and destination squares empty (the latter already enforced by DestinationIsEmpty in chain).
/// </summary>
/// <remarks>
/// This condition does not itself verify emptiness (handled earlier) but ensures: piece is pawn, distance == 2, and pawn has not moved before.
/// </remarks>
public sealed class PawnInitialDoubleStepGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Evaluates whether the supplied move event qualifies as an initial pawn double-step advance.
    /// </summary>
    /// <param name="engine">Game engine.</param>
    /// <param name="state">Current immutable state.</param>
    /// <param name="moveEvent">Move event.</param>
    /// <returns>Valid when criteria met, Ignore otherwise, or Fail when invariant missing.</returns>
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent moveEvent)
    {
        var @event = moveEvent; // local alias to retain prior variable name usage
        if (!@event.Piece.Id.Contains("pawn"))
        {
            return ConditionResponse.Ignore("Not a pawn");
        }

        if (@event.Distance != 2)
        {
            return ConditionResponse.Ignore("Not a double step");
        }

        // Require both steps be in the same forward direction for the pawn's color
        var dirs = @event.Path.Directions.ToArray();
        if (dirs.Length != 2 || dirs.Distinct().Count() != 1)
        {
            return ConditionResponse.Ignore("Not two uniform steps");
        }

        var dirId = dirs[0].Id;
        var isWhite = @event.Piece.Id.StartsWith("white-");
        if (isWhite && dirId != Constants.Directions.North)
        {
            return ConditionResponse.Ignore("White double-step must be north");
        }

        if (!isWhite && dirId != Constants.Directions.South)
        {
            return ConditionResponse.Ignore("Black double-step must be south");
        }

        // Starting rank enforcement (white from rank 2 -> 4; black from rank 7 -> 5)
        if (!ChessCoordinates.TryParse(@event.From.Id, out _, out var fromRank) || !ChessCoordinates.TryParse(@event.To.Id, out _, out var toRank))
        {
            return ConditionResponse.Ignore("Unparsable coordinates");
        }

        if (isWhite)
        {
            if (fromRank != 2 || toRank != 4)
            {
                return ConditionResponse.Ignore("White double-step not from rank 2 to 4");
            }
        }
        else
        {
            if (fromRank != 7 || toRank != 5)
            {
                return ConditionResponse.Ignore("Black double-step not from rank 7 to 5");
            }
        }

        var extras = state.GetExtras<ChessStateExtras>();
        if (extras is null)
        {
            return ConditionResponse.Fail("Missing chess extras");
        }

        // If piece already moved earlier, cannot double step now.
        if (extras.MovedPieceIds.Contains(@event.Piece.Id))
        {
            return ConditionResponse.Fail("Pawn already moved");
        }

        return ConditionResponse.Valid;
    }
}