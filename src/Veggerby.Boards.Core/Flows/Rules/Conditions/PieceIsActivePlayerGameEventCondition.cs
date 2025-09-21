using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions;

/// <summary>
/// Validates that the moving piece belongs to the currently active player.
/// </summary>
public class PieceIsActivePlayerGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var activePlayer = state.GetActivePlayer();
        return @event.Piece.Owner.Equals(activePlayer) ? ConditionResponse.Valid : ConditionResponse.Invalid;
    }
}