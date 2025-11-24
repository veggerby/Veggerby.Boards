using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Conditions;

/// <summary>
/// Condition that validates a piece is not in the base (already on the board).
/// </summary>
public class PieceNotInBaseCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var pieceState = state.GetState<PieceState>(@event.Piece);
        if (pieceState?.CurrentTile is null)
        {
            return ConditionResponse.Fail("Piece not on board");
        }

        // Check if piece is in base
        var owner = @event.Piece.Owner;
        if (owner is null)
        {
            return ConditionResponse.Fail("Piece has no owner");
        }

        var baseTileId = $"base-{owner.Id}";
        if (string.Equals(pieceState.CurrentTile.Id, baseTileId, StringComparison.Ordinal))
        {
            return ConditionResponse.Fail("Piece is in base, use enter event");
        }

        return ConditionResponse.Valid;
    }
}
