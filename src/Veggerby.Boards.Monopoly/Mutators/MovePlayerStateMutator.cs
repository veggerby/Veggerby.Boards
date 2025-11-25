using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that moves a player token on the board.
/// </summary>
public class MovePlayerStateMutator : IStateMutator<MovePlayerGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePlayerGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Find the player's piece
        var piece = engine.Game.Artifacts.OfType<Artifacts.Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, @event.Player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            throw new InvalidOperationException($"Piece not found for player {@event.Player.Id}");
        }

        // Get current position
        var currentState = gameState.GetState<PieceState>(piece);
        if (currentState?.CurrentTile is null)
        {
            throw new InvalidOperationException($"Current tile not found for player {@event.Player.Id}");
        }

        var currentPosition = MonopolyBoardConfiguration.GetPosition(currentState.CurrentTile.Id);
        var newPosition = (currentPosition + @event.Spaces) % 40;

        // Get the destination tile
        var newTileId = MonopolyBoardConfiguration.GetTileId(newPosition);
        var newTile = engine.Game.Board.GetTile(newTileId);

        if (newTile is null)
        {
            throw new InvalidOperationException($"Tile not found for position {newPosition}");
        }

        var newPieceState = new PieceState(piece, newTile);

        // Update consecutive doubles tracking
        var playerState = gameState.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is not null)
        {
            var newDoubles = @event.IsDoubles ? playerState.ConsecutiveDoubles + 1 : 0;
            var newPlayerState = playerState.WithConsecutiveDoubles(newDoubles);

            return gameState.Next([newPieceState, newPlayerState]);
        }

        return gameState.Next([newPieceState]);
    }
}
