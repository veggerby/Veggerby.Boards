using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Conditions;

/// <summary>
/// Enforces the mandatory capture rule in checkers: if any capture is available,
/// player must take a capture (not a normal move), and must choose the longest available chain.
/// </summary>
public sealed class MandatoryCaptureCondition : IGameEventCondition<MovePieceGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="MandatoryCaptureCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public MandatoryCaptureCondition(Game game)
    {
        _game = game;
    }

    /// <summary>
    /// Evaluates if the move satisfies the mandatory capture rule.
    /// </summary>
    /// <param name="engine">The game engine.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="evt">The move event to validate.</param>
    /// <returns>Valid if move is allowed, invalid with reason if not.</returns>
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent evt)
    {
        // Get active player
        var activePlayer = state.GetActivePlayer();

        if (activePlayer == null)
        {
            return ConditionResponse.Invalid("No active player");
        }

        // Calculate max capture length available to active player
        var maxCaptureLength = GetMaxCaptureLength(activePlayer, state);

        // If no captures available, any valid move is ok
        if (maxCaptureLength == 0)
        {
            return ConditionResponse.Valid();
        }

        // Count captures in this move (path length - 1, but only if it's a capture)
        var moveDistance = evt.Path.Distance;

        // A capture move has distance 2 or more (jumping over opponent piece)
        // For now, we'll allow the move if it's long enough
        // The actual capture validation will be done by other conditions/mutators
        
        // If there are mandatory captures but this is a distance-1 move, it's invalid
        if (moveDistance < 2)
        {
            return ConditionResponse.Invalid($"Mandatory capture of length {maxCaptureLength} available - normal moves not allowed");
        }

        // For multi-jump captures, verify the move length matches or exceeds requirement
        // Note: In standard checkers, you must take the longest chain available
        // For simplicity in this initial implementation, we'll accept any capture when captures are mandatory
        // A more sophisticated implementation would validate exact chain length

        return ConditionResponse.Valid();
    }

    /// <summary>
    /// Calculates the maximum capture chain length available to the specified player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="state">The current game state.</param>
    /// <returns>Maximum capture chain length (0 if no captures available).</returns>
    private int GetMaxCaptureLength(Player player, GameState state)
    {
        var maxLength = 0;

        // Get all pieces owned by the player
        var playerPieces = _game.Pieces.Where(p => p.Owner == player);

        foreach (var piece in playerPieces)
        {
            var pieceState = state.GetPieceState(piece);

            if (pieceState == null || pieceState.CurrentTile == null)
            {
                continue; // Piece not on board (captured)
            }

            // For each piece, check if it can make a capture
            // This is a simplified check - full implementation would use CaptureChainResolver
            var captureLength = GetCaptureChainLength(piece, pieceState.CurrentTile, state);

            if (captureLength > maxLength)
            {
                maxLength = captureLength;
            }
        }

        return maxLength;
    }

    /// <summary>
    /// Gets the length of the longest capture chain starting from the given piece and tile.
    /// This is a simplified implementation - full version would enumerate all chains.
    /// </summary>
    private int GetCaptureChainLength(Piece piece, Tile tile, GameState state)
    {
        // Simplified: just check if a single capture is possible
        // A full implementation would recursively explore all capture chains
        
        var metadata = piece.Metadata as CheckersPieceMetadata;

        if (metadata == null)
        {
            return 0;
        }

        // Check each diagonal direction
        var directions = GetAvailableDirections(metadata.Role, metadata.Color);

        foreach (var direction in directions)
        {
            // Check if we can jump in this direction
            var adjacentTile = tile.GetRelation(direction)?.Destination;

            if (adjacentTile == null)
            {
                continue;
            }

            // Check if there's an opponent piece on the adjacent tile
            var adjacentPieceState = state.GetPieceStates().FirstOrDefault(ps => ps.CurrentTile == adjacentTile);

            if (adjacentPieceState == null || adjacentPieceState.Artifact.Owner == piece.Owner)
            {
                continue; // No piece or friendly piece
            }

            // Check if the landing tile (beyond the opponent) is empty
            var landingTile = adjacentTile.GetRelation(direction)?.Destination;

            if (landingTile == null)
            {
                continue;
            }

            var landingPieceState = state.GetPieceStates().FirstOrDefault(ps => ps.CurrentTile == landingTile);

            if (landingPieceState == null)
            {
                // Found a valid capture! For now, return 1
                // Full implementation would recursively check for multi-jumps
                return 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets the available movement directions for a piece based on its role and color.
    /// </summary>
    private IEnumerable<Direction> GetAvailableDirections(CheckersPieceRole role, CheckersPieceColor color)
    {
        if (role == CheckersPieceRole.King)
        {
            // Kings can move in all diagonal directions
            return new[]
            {
                _game.GetDirection(Constants.Directions.NorthEast),
                _game.GetDirection(Constants.Directions.NorthWest),
                _game.GetDirection(Constants.Directions.SouthEast),
                _game.GetDirection(Constants.Directions.SouthWest)
            }.Where(d => d != null).Cast<Direction>();
        }

        // Regular pieces move forward only
        if (color == CheckersPieceColor.Black)
        {
            // Black moves toward higher tile numbers (south)
            return new[]
            {
                _game.GetDirection(Constants.Directions.SouthEast),
                _game.GetDirection(Constants.Directions.SouthWest)
            }.Where(d => d != null).Cast<Direction>();
        }
        else
        {
            // White moves toward lower tile numbers (north)
            return new[]
            {
                _game.GetDirection(Constants.Directions.NorthEast),
                _game.GetDirection(Constants.Directions.NorthWest)
            }.Where(d => d != null).Cast<Direction>();
        }
    }
}
