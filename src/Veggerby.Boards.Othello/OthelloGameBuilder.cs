using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Othello.Conditions;
using Veggerby.Boards.Othello.Mutators;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Concrete <see cref="GameBuilder"/> defining standard Othello/Reversi with disc flipping mechanics.
/// </summary>
/// <remarks>
/// Othello is played on an 8x8 board with two players (Black and White).
/// 
/// Rules:
/// - Black moves first
/// - Players place discs on empty squares
/// - A placement must flip at least one opponent disc
/// - Flipping occurs when the new disc creates a straight line (horizontal, vertical, or diagonal)
///   of opponent discs bounded by the new disc and another disc of the same color
/// - All sandwiched opponent discs are flipped to the player's color
/// - If a player has no valid move, they pass
/// - Game ends when the board is full or neither player can move
/// - Player with the most discs wins
/// 
/// Starting position:
///    a  b  c  d  e  f  g  h
/// 8  .  .  .  .  .  .  .  .
/// 7  .  .  .  .  .  .  .  .
/// 6  .  .  .  .  .  .  .  .
/// 5  .  .  .  W  B  .  .  .
/// 4  .  .  .  B  W  .  .  .
/// 3  .  .  .  .  .  .  .  .
/// 2  .  .  .  .  .  .  .  .
/// 1  .  .  .  .  .  .  .  .
/// </remarks>
public class OthelloGameBuilder : GameBuilder
{
    /// <summary>
    /// Configures the Othello board tiles, relations, discs, and phases.
    /// </summary>
    protected override void Build()
    {
        // Game
        BoardId = "othello";

        AddPlayer(OthelloIds.Players.Black);
        AddPlayer(OthelloIds.Players.White);

        // Black moves first in Othello
        WithActivePlayer(OthelloIds.Players.Black, true);
        WithActivePlayer(OthelloIds.Players.White, false);

        // All eight directions (orthogonal and diagonal)
        AddDirection(Constants.Directions.North);
        AddDirection(Constants.Directions.NorthEast);
        AddDirection(Constants.Directions.East);
        AddDirection(Constants.Directions.SouthEast);
        AddDirection(Constants.Directions.South);
        AddDirection(Constants.Directions.SouthWest);
        AddDirection(Constants.Directions.West);
        AddDirection(Constants.Directions.NorthWest);

        // Build 8x8 grid topology
        BuildBoardTopology();

        // Create discs for both players
        // In Othello, we need 64 discs total (32 per player, though not all may be used)
        // We'll create them but only place the initial 4
        CreateDiscs();

        // Set up initial position (4 discs in center)
        WithPiece("black-disc-1").OnTile(OthelloIds.Tiles.D5);
        WithPiece("black-disc-2").OnTile(OthelloIds.Tiles.E4);
        WithPiece("white-disc-1").OnTile(OthelloIds.Tiles.D4);
        WithPiece("white-disc-2").OnTile(OthelloIds.Tiles.E5);

        // Game phases and rules
        AddGamePhase("play")
            .WithEndGameDetection(
                game => new OthelloEndgameCondition(game),
                game => new OthelloEndGameMutator(game))
            .If(game => new GameNotEndedCondition())
            .Then()
                // Place disc (validates placement, flips opponent discs, changes turn)
                .ForEvent<PlaceDiscGameEvent>()
                    .If(game => new DiscIsActivePlayerCondition())
                        .And(game => new ValidPlacementCondition(game))
                .Then()
                    .Do(game => new PlaceDiscStateMutator())
                    .Do(game => new FlipDiscsStateMutator(game))
                    .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))

                // Pass turn (when player has no valid moves)
                .ForEvent<PassTurnGameEvent>()
                    .If(game => new PlayerIsActiveGameEventCondition())
                .Then()
                    .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()));
    }

    /// <summary>
    /// Builds the 8x8 grid topology for Othello board.
    /// Creates 64 tiles with orthogonal and diagonal connections.
    /// </summary>
    private void BuildBoardTopology()
    {
        var columns = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
        var rows = Enumerable.Range(1, 8);

        // Create all 64 tiles
        foreach (var row in rows)
        {
            foreach (var col in columns)
            {
                AddTile($"{col}{row}");
            }
        }

        // Add all connections (orthogonal and diagonal)
        foreach (var row in rows)
        {
            foreach (var col in columns)
            {
                var colIndex = System.Array.IndexOf(columns, col);
                var currentTile = $"{col}{row}";

                // North
                if (row < 8)
                {
                    WithTile(currentTile).WithRelationTo($"{col}{row + 1}").InDirection(Constants.Directions.North);
                }

                // South
                if (row > 1)
                {
                    WithTile(currentTile).WithRelationTo($"{col}{row - 1}").InDirection(Constants.Directions.South);
                }

                // East
                if (colIndex < 7)
                {
                    WithTile(currentTile).WithRelationTo($"{columns[colIndex + 1]}{row}").InDirection(Constants.Directions.East);
                }

                // West
                if (colIndex > 0)
                {
                    WithTile(currentTile).WithRelationTo($"{columns[colIndex - 1]}{row}").InDirection(Constants.Directions.West);
                }

                // NorthEast
                if (row < 8 && colIndex < 7)
                {
                    WithTile(currentTile).WithRelationTo($"{columns[colIndex + 1]}{row + 1}").InDirection(Constants.Directions.NorthEast);
                }

                // NorthWest
                if (row < 8 && colIndex > 0)
                {
                    WithTile(currentTile).WithRelationTo($"{columns[colIndex - 1]}{row + 1}").InDirection(Constants.Directions.NorthWest);
                }

                // SouthEast
                if (row > 1 && colIndex < 7)
                {
                    WithTile(currentTile).WithRelationTo($"{columns[colIndex + 1]}{row - 1}").InDirection(Constants.Directions.SouthEast);
                }

                // SouthWest
                if (row > 1 && colIndex > 0)
                {
                    WithTile(currentTile).WithRelationTo($"{columns[colIndex - 1]}{row - 1}").InDirection(Constants.Directions.SouthWest);
                }
            }
        }
    }

    /// <summary>
    /// Creates disc pieces for both players.
    /// </summary>
    private void CreateDiscs()
    {
        // Create 32 black discs
        AddMultiplePieces(32, i => $"black-disc-{i + 1}", (piece, index) =>
        {
            piece
                .WithOwner(OthelloIds.Players.Black)
                .WithMetadata(new OthelloDiscMetadata(OthelloDiscColor.Black));
        });

        // Create 32 white discs
        AddMultiplePieces(32, i => $"white-disc-{i + 1}", (piece, index) =>
        {
            piece
                .WithOwner(OthelloIds.Players.White)
                .WithMetadata(new OthelloDiscMetadata(OthelloDiscColor.White));
        });
    }
}
