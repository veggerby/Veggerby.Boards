using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Ensures a move does not target a tile occupied by one of the moving player's own pieces.
/// </summary>
/// <remarks>
/// Returns <see cref="ConditionResponse.Ignore(string)"/> when a friendly piece occupies the destination so the move is silently ignored (non-exceptional).
/// Opponent occupancy is permitted and handled separately by capture semantics.
/// </remarks>
public sealed class DestinationNotOwnPieceGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var anyFriendly = state.GetPiecesOnTile(@event.To).Any(ps => ps.Owner?.Equals(@event.Piece.Owner) ?? false);
        return anyFriendly
            ? ConditionResponse.Ignore("Destination occupied by friendly piece")
            : ConditionResponse.Valid;
    }
}