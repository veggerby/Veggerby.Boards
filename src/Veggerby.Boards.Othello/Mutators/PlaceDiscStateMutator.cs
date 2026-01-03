using System;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello.Mutators;

/// <summary>
/// Places a disc on the board when a <see cref="PlaceDiscGameEvent"/> is processed.
/// </summary>
public sealed class PlaceDiscStateMutator : IStateMutator<PlaceDiscGameEvent>
{
    /// <summary>
    /// Places the disc on the target tile.
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, PlaceDiscGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.Disc == null || @event.Target == null)
        {
            return gameState;
        }

        var newPieceState = new PieceState(@event.Disc, @event.Target);

        return gameState.Next(newPieceState);
    }
}
