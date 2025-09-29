using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Condition that is Valid when the moving piece is not a pawn; otherwise Ignore.
/// Used to route generic piece movement (rook, knight, bishop, queen, king) ahead of pawn-specific rule branches.
/// </summary>
public sealed class NonPawnGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        return @event.Piece.Id.Contains("pawn")
            ? ConditionResponse.Ignore("Is pawn")
            : ConditionResponse.Valid;
    }
}