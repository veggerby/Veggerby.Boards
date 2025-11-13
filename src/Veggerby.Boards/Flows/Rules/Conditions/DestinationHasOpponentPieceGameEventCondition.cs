using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Determines if the destination tile contains an opponent piece (capture candidate).
/// </summary>
/// <remarks>
/// Returns Valid when at least one opponent piece occupies the destination. Returns Ignore otherwise so that
/// non-capture rules may still evaluate (e.g., normal movement onto empty tile or friendly-blocked tile logic).
/// </remarks>
public sealed class DestinationHasOpponentPieceGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var anyOpponent = state.GetPiecesOnTile(@event.To).Any(ps => ps.Owner is not null && !ps.Owner.Equals(@event.Piece.Owner));
        return anyOpponent
            ? ConditionResponse.Valid
            : ConditionResponse.Ignore("No opponent piece on destination tile");
    }
}