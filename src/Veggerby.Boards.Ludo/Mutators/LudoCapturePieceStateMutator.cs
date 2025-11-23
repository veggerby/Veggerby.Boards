using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Mutators;

/// <summary>
/// Mutator that handles piece capture in Ludo (sends opponent pieces back to base).
/// Only captures on non-safe squares.
/// </summary>
public class LudoCapturePieceStateMutator : IStateMutator<MovePieceGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var destinationTile = @event.To;

        // Check if destination is a safe square
        var safeSquaresState = gameState.GetState<LudoSafeSquaresState>(LudoSafeSquaresMarker.Instance);
        if (safeSquaresState is not null && safeSquaresState.IsSafeSquare(destinationTile.Id))
        {
            // No capture on safe squares
            return gameState;
        }

        // Check for opponent pieces on destination
        var opponentPieces = gameState.GetStates<PieceState>()
            .Where(ps => ps.CurrentTile is not null &&
                         ps.CurrentTile.Equals(destinationTile) &&
                         ps.Artifact.Owner is not null &&
                         !ps.Artifact.Owner.Equals(@event.Piece.Owner))
            .ToList();

        if (opponentPieces.Count == 0)
        {
            // No capture needed
            return gameState;
        }

        // Send opponent pieces back to their base
        var updates = new System.Collections.Generic.List<PieceState>();
        foreach (var opponentPieceState in opponentPieces)
        {
            var opponentOwner = opponentPieceState.Artifact.Owner;
            if (opponentOwner is null)
            {
                continue;
            }

            var baseTileId = $"base-{opponentOwner.Id}";
            var baseTile = engine.Game.GetTile(baseTileId);
            if (baseTile is null)
            {
                continue;
            }

            updates.Add(new PieceState(opponentPieceState.Artifact, baseTile));
        }

        return updates.Count > 0 ? gameState.Next(updates) : gameState;
    }
}
