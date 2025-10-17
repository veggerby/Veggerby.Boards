using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go.Mutators;

/// <summary>
/// Applies a <see cref="PlaceStoneGameEvent"/>, producing new piece + possible captured opponent stones removal.
/// Capture logic (group/liberty evaluation) is deferred; current version only places on empty tile.
/// </summary>
public sealed class PlaceStoneStateMutator : IStateMutator<PlaceStoneGameEvent>
{
    /// <summary>
    /// Places the stone if the intersection is empty, resetting ko / pass counters.
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, PlaceStoneGameEvent @event)
    {
        // Reject placement if tile occupied
        if (gameState.GetStates<PieceState>().Any(ps => ps.CurrentTile.Equals(@event.Target)))
        {
            return gameState; // silent invalid (rule layer can refine later)
        }

        var newPieceState = new PieceState(@event.Stone, @event.Target);

        // Reset ko & pass count on placement
        var extras = gameState.GetExtras<GoStateExtras>() ?? new GoStateExtras(null, 0, 19);
        var updatedExtras = extras with { KoTileId = null, ConsecutivePasses = 0 };
        var withExtras = gameState.ReplaceExtras(updatedExtras);
        return withExtras.Next([newPieceState]);
    }
}