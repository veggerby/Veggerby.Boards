using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Conditions;

/// <summary>
/// Detects when a checkers game has reached a terminal state:
/// - Active player has no valid moves (loses)
/// - Active player has no pieces remaining (loses)
/// </summary>
public sealed class CheckersEndgameCondition : IGameStateCondition
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckersEndgameCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public CheckersEndgameCondition(Game game)
    {
        _game = game;
    }

    /// <summary>
    /// Evaluates whether the game has ended.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <returns>Valid if game ended, Ignore if still in progress.</returns>
    public ConditionResponse Evaluate(GameState state)
    {
        var activePlayer = state.GetActivePlayer();

        if (activePlayer == null)
        {
            return ConditionResponse.Ignore("No active player");
        }

        // Check if active player has any pieces remaining
        var activePieces = _game.Pieces
            .Where(p => p.Owner == activePlayer)
            .Select(p => state.GetPieceState(p))
            .Where(ps => ps != null && ps.CurrentTile != null)
            .ToList();

        if (activePieces.Count == 0)
        {
            // No pieces remaining - opponent wins
            return ConditionResponse.Valid();
        }

        // Check if active player has any valid moves
        // Simplified: just check if any piece can move in any direction
        var hasValidMove = HasAnyValidMove(activePlayer, state);

        if (!hasValidMove)
        {
            // No valid moves - opponent wins
            return ConditionResponse.Valid();
        }

        // Game still in progress
        return ConditionResponse.Ignore("Game not ended");
    }

    /// <summary>
    /// Checks if the active player has any valid move available.
    /// </summary>
    private bool HasAnyValidMove(Player player, GameState state)
    {
        var playerPieces = _game.Pieces.Where(p => p.Owner == player);

        foreach (var piece in playerPieces)
        {
            var pieceState = state.GetPieceState(piece);

            if (pieceState == null || pieceState.CurrentTile == null)
            {
                continue;
            }

            var metadata = piece.Metadata as CheckersPieceMetadata;

            if (metadata == null)
            {
                continue;
            }

            // Get available directions based on piece type
            var directions = GetAvailableDirections(metadata.Role, metadata.Color);

            foreach (var direction in directions)
            {
                // Check for normal move
                var adjacentTile = pieceState.CurrentTile.GetRelation(direction)?.Destination;

                if (adjacentTile != null)
                {
                    var adjacentPieceState = state.GetPieceStates().FirstOrDefault(ps => ps.CurrentTile == adjacentTile);

                    if (adjacentPieceState == null)
                    {
                        // Empty adjacent tile - valid move
                        return true;
                    }

                    // Check for capture move
                    if (adjacentPieceState.Artifact.Owner != player)
                    {
                        var landingTile = adjacentTile.GetRelation(direction)?.Destination;

                        if (landingTile != null)
                        {
                            var landingPieceState = state.GetPieceStates().FirstOrDefault(ps => ps.CurrentTile == landingTile);

                            if (landingPieceState == null)
                            {
                                // Valid capture move
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the available movement directions for a piece based on its role and color.
    /// </summary>
    private IEnumerable<Direction> GetAvailableDirections(CheckersPieceRole role, CheckersPieceColor color)
    {
        if (role == CheckersPieceRole.King)
        {
            return new[]
            {
                _game.GetDirection(Constants.Directions.NorthEast),
                _game.GetDirection(Constants.Directions.NorthWest),
                _game.GetDirection(Constants.Directions.SouthEast),
                _game.GetDirection(Constants.Directions.SouthWest)
            }.Where(d => d != null).Cast<Direction>();
        }

        if (color == CheckersPieceColor.Black)
        {
            return new[]
            {
                _game.GetDirection(Constants.Directions.SouthEast),
                _game.GetDirection(Constants.Directions.SouthWest)
            }.Where(d => d != null).Cast<Direction>();
        }
        else
        {
            return new[]
            {
                _game.GetDirection(Constants.Directions.NorthEast),
                _game.GetDirection(Constants.Directions.NorthWest)
            }.Where(d => d != null).Cast<Direction>();
        }
    }
}
