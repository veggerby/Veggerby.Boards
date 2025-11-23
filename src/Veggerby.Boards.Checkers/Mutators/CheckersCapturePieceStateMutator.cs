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

        // For each segment in the path, check if a piece was jumped over
        // In checkers, if the distance is 2 (jumping), there's a piece in between
        for (int i = 0; i < path.Relations.Count; i++)
        {
            var relation = path.Relations[i];
            
            // Check if this is a jump (distance > 1 means jumping over a tile)
            if (relation.Distance > 1)
            {
                // Find the jumped tile (intermediate tile between from and to)
                var jumpedPiece = FindJumpedPiece(gameState, relation.From, relation.To, relation.Direction);
                if (jumpedPiece != null && jumpedPiece.Owner != null && !jumpedPiece.Owner.Equals(@event.Piece.Owner))
                {
                    // This is an opponent piece that was jumped over
                    capturedPieces.Add(new CapturedPieceState(jumpedPiece));
                }
            }
        }

        if (capturedPieces.Count == 0)
        {
            return gameState; // No captures occurred
        }

        // Apply all captures to the game state
        return gameState.Next(capturedPieces.ToArray());
    }

    /// <summary>
    /// Finds the piece that was jumped over between two tiles.
    /// </summary>
    /// <param name="gameState">Current game state.</param>
    /// <param name="from">The tile the piece jumped from.</param>
    /// <param name="to">The tile the piece landed on.</param>
    /// <param name="direction">The direction of the jump.</param>
    /// <returns>The piece that was jumped over, or null if no piece was jumped.</returns>
    private Piece? FindJumpedPiece(GameState gameState, Tile from, Tile to, Direction direction)
    {
        // In checkers, when jumping, the jumped piece is on the tile directly between from and to
        // Get the intermediate tile by following the direction once from 'from'
        var intermediateRelation = _game.Board.GetTileRelation(from, direction);
        if (intermediateRelation == null)
        {
            return null; // No intermediate tile found
        }

        var intermediateTile = intermediateRelation.To;
        
        // Verify that this intermediate tile connects to our destination in the same direction
        var nextRelation = _game.Board.GetTileRelation(intermediateTile, direction);
        if (nextRelation == null || !nextRelation.To.Equals(to))
        {
            return null; // Path doesn't match expected jump geometry
        }

        // Get the piece on the intermediate tile
        var piecesOnTile = gameState.GetPiecesOnTile(intermediateTile);
        return piecesOnTile.FirstOrDefault();
    }
}
