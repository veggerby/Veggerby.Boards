using System;

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
        ArgumentNullException.ThrowIfNull(engine, nameof(engine));
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(@event, nameof(@event));

        var rolesExtras = state.GetExtras<ChessPieceRolesExtras>();
        if (ChessPiece.IsPawn(state, @event.Piece.Id))
        {
            return ConditionResponse.Ignore("Pawn excluded");
        }
        return ConditionResponse.Valid;
    }
}