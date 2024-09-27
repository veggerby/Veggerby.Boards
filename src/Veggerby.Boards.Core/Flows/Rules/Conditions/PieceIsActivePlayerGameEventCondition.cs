using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions;

public class PieceIsActivePlayerGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        var activePlayer = state.GetActivePlayer();
        return @event.Piece.Owner.Equals(activePlayer) ? ConditionResponse.Valid : ConditionResponse.Invalid;
    }
}