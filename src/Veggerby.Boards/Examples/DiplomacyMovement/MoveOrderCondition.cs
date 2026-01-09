using System;

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
        // Avoid LINQ in performance-sensitive condition evaluation - use explicit iteration
        States.PieceState? currentPosition = null;
        foreach (var ps in gameState.GetStates<States.PieceState>())
        {
            if (ps.Artifact.Equals(@event.Unit))
            {
                currentPosition = ps;
                break;
            }
        }

        if (currentPosition is null)
        {
            return ConditionResponse.Invalid;
        }

        // Check if destination is already occupied by another piece
        foreach (var ps in gameState.GetStates<States.PieceState>())
        {
            if (ps.CurrentTile.Equals(@event.Destination))
            {
                if (!ps.Artifact.Equals(@event.Unit))
                {
                    // Conflict: another unit already at destination
                    // The player-order tie-breaking means the first commit wins
                    return ConditionResponse.Invalid;
                }

                break;
            }
        }

        return ConditionResponse.Valid;
    }
}
