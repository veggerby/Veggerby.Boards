using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Validates that the moving piece belongs to the currently active player.
/// </summary>
public class PieceIsActivePlayerGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var activePlayer = state.GetActivePlayer();
        // Returning Ignore (rather than Invalid) allows UI/consumers to attempt moves optimistically
        // without triggering exceptions for simple "not your turn" cases, aligning with other
        // non-applicable movement attempts (e.g., friendly destination). Determinism preserved.
        return @event.Piece.Owner.Equals(activePlayer) ? ConditionResponse.Valid : ConditionResponse.Ignore("Inactive player turn");
    }
}