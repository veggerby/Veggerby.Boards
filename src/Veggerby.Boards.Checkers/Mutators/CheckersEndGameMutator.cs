using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Mutators;

/// <summary>
/// Mutator that adds game termination states when checkers game ends.
/// Determines the winner and creates appropriate outcome state.
/// </summary>
public sealed class CheckersEndGameMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckersEndGameMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public CheckersEndGameMutator(Game game)
    {
        _game = game;
    }

    /// <summary>
    /// Mutates the state to add game ended and outcome states if game is over.
    /// </summary>
    /// <param name="engine">The game engine.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="event">The triggering event.</param>
    /// <returns>New state with game ended markers if applicable.</returns>
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        // Check if already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return state;
        }

        var activePlayer = state.GetActivePlayer();

        if (activePlayer == null)
        {
            return state;
        }

        // Check if active player has any pieces remaining
        var activePieces = _game.Pieces
            .Where(p => p.Owner == activePlayer)
            .Select(p => state.GetPieceState(p))
            .Where(ps => ps != null && ps.CurrentTile != null)
            .ToList();

        // Determine winner (opponent of active player who cannot move)
        Player? winner = null;

        if (activePieces.Count == 0 || !HasAnyValidMove(activePlayer, state))
        {
            // Active player loses (no pieces or no moves)
            winner = _game.Players.FirstOrDefault(p => p != activePlayer);
        }
        else
        {
            // Game not over yet
            return state;
        }

        // Create outcome state
        var outcomeState = new CheckersOutcomeState(winner, _game.Players);

        // Add both GameEndedState and outcome state
        return state.Next(new IArtifactState[]
        {
            new GameEndedState(),
            outcomeState
        });
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

            var directions = GetAvailableDirections(metadata.Role, metadata.Color);

            foreach (var direction in directions)
            {
                var adjacentTile = pieceState.CurrentTile.GetRelation(direction)?.Destination;

                if (adjacentTile != null)
                {
                    var adjacentPieceState = state.GetPieceStates().FirstOrDefault(ps => ps.CurrentTile == adjacentTile);

                    if (adjacentPieceState == null)
                    {
                        return true;
                    }

                    if (adjacentPieceState.Artifact.Owner != player)
                    {
                        var landingTile = adjacentTile.GetRelation(direction)?.Destination;

                        if (landingTile != null)
                        {
                            var landingPieceState = state.GetPieceStates().FirstOrDefault(ps => ps.CurrentTile == landingTile);

                            if (landingPieceState == null)
                            {
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
