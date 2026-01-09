using System;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.DiplomacyMovement;

/// <summary>
/// Mutator that applies a move order by updating the unit's position.
/// </summary>
/// <remarks>
/// In a full Diplomacy implementation, this would also handle:
/// - Support orders (units assisting other units' moves)
/// - Hold orders (units staying in place defensively)
/// - Convoy orders (naval transport)
/// - Bounces (when multiple units move to same location)
/// 
/// This simplified version just moves the unit to the destination if possible.
/// Conflicts are resolved by the commit/reveal system's player-order tie-breaking:
/// the first player (by ID order) to commit a move to a destination succeeds,
/// subsequent moves to the same destination fail validation.
/// </remarks>
internal sealed class MoveOrderStateMutator : IStateMutator<MoveOrderEvent>
{
    public GameState MutateState(GameEngine engine, GameState gameState, MoveOrderEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var currentPosition = gameState.GetStates<PieceState>()
            .FirstOrDefault(ps => ps.Artifact.Equals(@event.Unit));

        if (currentPosition is null)
        {
            return gameState;
        }

        // Move the unit to the destination
        var newPosition = new PieceState(@event.Unit, @event.Destination);
        return gameState.Next([newPosition]);
    }
}
