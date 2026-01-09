using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.DiplomacyMovement;

/// <summary>
/// Condition validating that a move order is legal.
/// </summary>
/// <remarks>
/// This simplified version just checks that the event is properly formed.
/// In a full Diplomacy implementation, this would validate:
/// - Player owns the unit
/// - Destination is adjacent to unit's current location
/// - Destination is not occupied by friendly unit
/// - Complex conflict resolution with support orders
/// 
/// In the commit/reveal pattern, this validation occurs during reveal phase when the
/// committed move orders are applied.
/// </remarks>
internal sealed class MoveOrderCondition : IGameEventCondition<MoveOrderEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState gameState, MoveOrderEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Simplified validation: just check that destination is not currently occupied by the same unit
        // In a real implementation, this would check adjacency, ownership, etc.
        var currentPosition = gameState.GetStates<States.PieceState>()
            .FirstOrDefault(ps => ps.Artifact.Equals(@event.Unit));

        if (currentPosition is null)
        {
            return ConditionResponse.Invalid;
        }

        // Check if destination is already occupied by another piece
        var occupant = gameState.GetStates<States.PieceState>()
            .FirstOrDefault(ps => ps.CurrentTile.Equals(@event.Destination));

        if (occupant is not null && !occupant.Artifact.Equals(@event.Unit))
        {
            // Conflict: another unit already at destination
            // The player-order tie-breaking means the first commit wins
            return ConditionResponse.Invalid;
        }

        return ConditionResponse.Valid;
    }
}
