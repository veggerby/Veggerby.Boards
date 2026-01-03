using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello.Conditions;

/// <summary>
/// Detects when an Othello game has ended.
/// </summary>
/// <remarks>
/// An Othello game ends when:
/// - The board is completely full, OR
/// - Neither player has a valid move (both must pass)
/// </remarks>
public sealed class OthelloEndgameCondition : IGameStateCondition
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="OthelloEndgameCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public OthelloEndgameCondition(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Check if game has already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return ConditionResponse.Ignore("Game already ended");
        }

        // Check if board is full
        var allTiles = _game.Board.Tiles;
        var occupiedTiles = allTiles.Where(tile => state.GetPiecesOnTile(tile).Any());
        var occupiedCount = occupiedTiles.Count();

        if (occupiedCount == allTiles.Count())
        {
            return ConditionResponse.Valid; // Board is full
        }

        // Check if both players have no valid moves
        var blackPlayer = _game.GetPlayer(OthelloIds.Players.Black);
        var whitePlayer = _game.GetPlayer(OthelloIds.Players.White);

        if (blackPlayer == null || whitePlayer == null)
        {
            return ConditionResponse.Ignore("Players not found");
        }

        var blackHasMove = HasAnyValidMove(state, blackPlayer);
        var whiteHasMove = HasAnyValidMove(state, whitePlayer);

        if (!blackHasMove && !whiteHasMove)
        {
            return ConditionResponse.Valid; // Neither player can move
        }

        return ConditionResponse.Ignore("Game continues");
    }

    private bool HasAnyValidMove(GameState state, Player player)
    {
        // Get all empty tiles
        var emptyTiles = _game.Board.Tiles.Where(t => !state.GetPiecesOnTile(t).Any());

        // Get a disc for this player to test placements
        var playerColor = player.Id == OthelloIds.Players.Black ? OthelloDiscColor.Black : OthelloDiscColor.White;
        var opponentColor = playerColor == OthelloDiscColor.Black ? OthelloDiscColor.White : OthelloDiscColor.Black;

        return emptyTiles.Any(tile =>
        {
            var relations = _game.Board.TileRelations.Where(r => r.From.Id == tile.Id);
            return relations.Any(relation =>
                WouldFlipInDirection(state, tile, playerColor, relation.Direction, opponentColor));
        });
    }

    private bool WouldFlipInDirection(GameState state, Tile startTile, OthelloDiscColor playerColor, Direction direction, OthelloDiscColor opponentColor)
    {
        var currentTile = startTile;
        var opponentPiecesFound = 0;

        // Move in the direction, counting opponent pieces
        while (true)
        {
            var relation = _game.Board.GetTileRelation(currentTile, direction);
            if (relation == null)
            {
                return false;
            }

            currentTile = relation.To;
            var piecesOnTile = state.GetPiecesOnTile(currentTile).ToList();

            if (!piecesOnTile.Any())
            {
                return false;
            }

            var piece = piecesOnTile.First();
            var currentColor = OthelloHelper.GetCurrentDiscColor(piece, state);

            if (currentColor == opponentColor)
            {
                opponentPiecesFound++;
            }
            else if (currentColor == playerColor)
            {
                return opponentPiecesFound > 0;
            }
        }
    }
}
