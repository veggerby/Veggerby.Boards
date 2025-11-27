using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that moves a player to a specific position on the board.
/// Used for card effects like "Advance to Go", "Go directly to Jail", etc.
/// </summary>
public class MoveToPositionStateMutator : IStateMutator<MoveToPositionGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, MoveToPositionGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Find the player's piece
        var piece = engine.Game.Artifacts.OfType<Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, @event.Player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            throw new InvalidOperationException($"Piece not found for player {@event.Player.Id}");
        }

        // Get current position
        var currentState = state.GetState<PieceState>(piece);
        if (currentState?.CurrentTile is null)
        {
            throw new InvalidOperationException($"Current tile not found for player {@event.Player.Id}");
        }

        var currentPosition = MonopolyBoardConfiguration.GetPosition(currentState.CurrentTile.Id);
        var newPosition = @event.TargetPosition;

        // Determine if player passes Go (not when going to jail)
        var passesGo = @event.CollectGoIfPassing && newPosition != 10 &&
                       (newPosition < currentPosition || (newPosition == 0 && currentPosition > 0));

        // Get the destination tile
        var newTileId = MonopolyBoardConfiguration.GetTileId(newPosition);
        var newTile = engine.Game.Board.GetTile(newTileId);

        if (newTile is null)
        {
            throw new InvalidOperationException($"Tile not found for position {newPosition}");
        }

        var newPieceState = new PieceState(piece, newTile);
        var updates = new List<IArtifactState> { newPieceState };

        // Handle passing Go
        if (passesGo)
        {
            var playerState = state.GetStates<MonopolyPlayerState>()
                .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

            if (playerState is not null)
            {
                updates.Add(playerState.AdjustCash(200));
            }
        }

        return state.Next(updates);
    }
}
