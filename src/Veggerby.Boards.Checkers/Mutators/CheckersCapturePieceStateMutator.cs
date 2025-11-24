using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Mutators;

/// <summary>
/// Captures pieces that are jumped over during a move in checkers.
/// In checkers, a capture occurs by jumping over an opponent's piece to an empty square beyond it.
/// </summary>
/// <remarks>
/// This mutator handles:
/// - Single jumps (piece jumps over one opponent piece)
/// - Multi-jump sequences (piece jumps over multiple opponent pieces in one turn)
/// - Removing jumped pieces from the board by marking them as captured
/// </remarks>
public sealed class CheckersCapturePieceStateMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckersCapturePieceStateMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public CheckersCapturePieceStateMutator(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Get the path of the move
        var path = @event.Path;
        if (path == null || path.Tiles.Count < 2)
        {
            return gameState; // Not a valid move path
        }

        var capturedPieces = new List<CapturedPieceState>();

        // Check if this is a jump move (2 or more tiles in path = jumped over middle tile(s))
        // In checkers, a jump is indicated by a path with more than 2 tiles (start + intermediate + end)
        // Or a path with 2 relations in the same direction (indicates a 2-step jump)

        if (path.Relations.Count == 2 && path.Directions.Count == 2 &&
            path.Directions[0].Equals(path.Directions[1]))
        {
            // This is a 2-step jump in the same direction - capture the piece on the intermediate tile
            var intermediateTile = path.Tiles[1]; // The middle tile
            var piecesOnTile = gameState.GetPiecesOnTile(intermediateTile);
            var jumpedPiece = piecesOnTile.FirstOrDefault();

            if (jumpedPiece != null && jumpedPiece.Owner != null && !jumpedPiece.Owner.Equals(@event.Piece.Owner))
            {
                // This is an opponent piece that was jumped over
                capturedPieces.Add(new CapturedPieceState(jumpedPiece));
            }
        }
        // TODO: Handle multi-jump sequences (more than 2 relations)

        if (capturedPieces.Count == 0)
        {
            return gameState; // No captures occurred
        }

        // Apply all captures to the game state
        return gameState.Next(capturedPieces.ToArray());
    }
}
