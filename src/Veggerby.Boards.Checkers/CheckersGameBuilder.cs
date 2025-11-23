using Veggerby.Boards.Checkers.Conditions;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Checkers;

/// <summary>
/// Concrete <see cref="GameBuilder"/> defining standard checkers with dark-square topology,
/// mandatory multi-jump capture mechanics, and king promotion.
/// </summary>
/// <remarks>
/// Board layout uses standard checkers numbering (1-32) for dark squares only.
/// Black pieces start on rows 1-3 (tiles 1-12), white pieces on rows 6-8 (tiles 21-32).
/// 
/// Standard checkers board (dark squares only):
/// Row 8:  29  30  31  32     BLACK PROMOTION ROW
/// Row 7:    25  26  27  28
/// Row 6:  21  22  23  24
/// Row 5:    17  18  19  20
/// Row 4:  13  14  15  16
/// Row 3:    9   10  11  12
/// Row 2:  5   6   7   8
/// Row 1:    1   2   3   4    WHITE PROMOTION ROW
/// 
/// Movement patterns:
/// - Regular pieces: forward diagonal only (NE/NW for black, SE/SW for white)
/// - Kings: all four diagonal directions (NE/NW/SE/SW)
/// - Captures: jump over opponent piece to empty square (can chain multiple jumps)
/// - Mandatory capture: must take longest available capture chain
/// </remarks>
public class CheckersGameBuilder : GameBuilder
{
    /// <summary>
    /// Configures the checkers board tiles, relations, pieces, movement patterns and phases.
    /// </summary>
    protected override void Build()
    {
        // Game
        BoardId = "checkers";

        AddPlayer(CheckersIds.Players.Black);
        AddPlayer(CheckersIds.Players.White);

        // Black moves first in checkers
        WithActivePlayer(CheckersIds.Players.Black, true);
        WithActivePlayer(CheckersIds.Players.White, false);

        // Diagonal directions only (checkers uses dark squares exclusively)
        AddDirection(Constants.Directions.NorthEast);
        AddDirection(Constants.Directions.NorthWest);
        AddDirection(Constants.Directions.SouthEast);
        AddDirection(Constants.Directions.SouthWest);

        // Build dark-square only topology (32 playable squares)
        BuildDarkSquareTopology();

        // Black pieces (regular pieces, can be promoted to kings)
        AddMultiplePieces(12, i => $"black-piece-{i + 1}", (piece, index) =>
        {
            piece
                .WithOwner(CheckersIds.Players.Black)
                .WithMetadata(new CheckersPieceMetadata(CheckersPieceRole.Regular, CheckersPieceColor.Black))
                // All diagonal moves defined (KingMovementCondition restricts regular pieces to forward only)
                // Single-step normal moves (all diagonals)
                .HasDirection(Constants.Directions.SouthEast).Done()
                .HasDirection(Constants.Directions.SouthWest).Done()
                .HasDirection(Constants.Directions.NorthEast).Done()
                .HasDirection(Constants.Directions.NorthWest).Done()
                // Two-step jump moves (for captures - all diagonals)
                .HasPattern(Constants.Directions.SouthEast, Constants.Directions.SouthEast)
                .HasPattern(Constants.Directions.SouthWest, Constants.Directions.SouthWest)
                .HasPattern(Constants.Directions.NorthEast, Constants.Directions.NorthEast)
                .HasPattern(Constants.Directions.NorthWest, Constants.Directions.NorthWest);
        });

        // White pieces (regular pieces, can be promoted to kings)
        AddMultiplePieces(12, i => $"white-piece-{i + 1}", (piece, index) =>
        {
            piece
                .WithOwner(CheckersIds.Players.White)
                .WithMetadata(new CheckersPieceMetadata(CheckersPieceRole.Regular, CheckersPieceColor.White))
                // All diagonal moves defined (KingMovementCondition restricts regular pieces to forward only)
                // Single-step normal moves (all diagonals)
                .HasDirection(Constants.Directions.NorthEast).Done()
                .HasDirection(Constants.Directions.NorthWest).Done()
                .HasDirection(Constants.Directions.SouthEast).Done()
                .HasDirection(Constants.Directions.SouthWest).Done()
                // Two-step jump moves (for captures - all diagonals)
                .HasPattern(Constants.Directions.NorthEast, Constants.Directions.NorthEast)
                .HasPattern(Constants.Directions.NorthWest, Constants.Directions.NorthWest)
                .HasPattern(Constants.Directions.SouthEast, Constants.Directions.SouthEast)
                .HasPattern(Constants.Directions.SouthWest, Constants.Directions.SouthWest);
        });

        // Initial piece positions
        // Black pieces on tiles 1-12 (rows 1-3)
        WithPiece(CheckersIds.Pieces.BlackPiece1).OnTile(CheckersIds.Tiles.Tile1);
        WithPiece(CheckersIds.Pieces.BlackPiece2).OnTile(CheckersIds.Tiles.Tile2);
        WithPiece(CheckersIds.Pieces.BlackPiece3).OnTile(CheckersIds.Tiles.Tile3);
        WithPiece(CheckersIds.Pieces.BlackPiece4).OnTile(CheckersIds.Tiles.Tile4);
        WithPiece(CheckersIds.Pieces.BlackPiece5).OnTile(CheckersIds.Tiles.Tile5);
        WithPiece(CheckersIds.Pieces.BlackPiece6).OnTile(CheckersIds.Tiles.Tile6);
        WithPiece(CheckersIds.Pieces.BlackPiece7).OnTile(CheckersIds.Tiles.Tile7);
        WithPiece(CheckersIds.Pieces.BlackPiece8).OnTile(CheckersIds.Tiles.Tile8);
        WithPiece(CheckersIds.Pieces.BlackPiece9).OnTile(CheckersIds.Tiles.Tile9);
        WithPiece(CheckersIds.Pieces.BlackPiece10).OnTile(CheckersIds.Tiles.Tile10);
        WithPiece(CheckersIds.Pieces.BlackPiece11).OnTile(CheckersIds.Tiles.Tile11);
        WithPiece(CheckersIds.Pieces.BlackPiece12).OnTile(CheckersIds.Tiles.Tile12);

        // White pieces on tiles 21-32 (rows 6-8)
        WithPiece(CheckersIds.Pieces.WhitePiece1).OnTile(CheckersIds.Tiles.Tile21);
        WithPiece(CheckersIds.Pieces.WhitePiece2).OnTile(CheckersIds.Tiles.Tile22);
        WithPiece(CheckersIds.Pieces.WhitePiece3).OnTile(CheckersIds.Tiles.Tile23);
        WithPiece(CheckersIds.Pieces.WhitePiece4).OnTile(CheckersIds.Tiles.Tile24);
        WithPiece(CheckersIds.Pieces.WhitePiece5).OnTile(CheckersIds.Tiles.Tile25);
        WithPiece(CheckersIds.Pieces.WhitePiece6).OnTile(CheckersIds.Tiles.Tile26);
        WithPiece(CheckersIds.Pieces.WhitePiece7).OnTile(CheckersIds.Tiles.Tile27);
        WithPiece(CheckersIds.Pieces.WhitePiece8).OnTile(CheckersIds.Tiles.Tile28);
        WithPiece(CheckersIds.Pieces.WhitePiece9).OnTile(CheckersIds.Tiles.Tile29);
        WithPiece(CheckersIds.Pieces.WhitePiece10).OnTile(CheckersIds.Tiles.Tile30);
        WithPiece(CheckersIds.Pieces.WhitePiece11).OnTile(CheckersIds.Tiles.Tile31);
        WithPiece(CheckersIds.Pieces.WhitePiece12).OnTile(CheckersIds.Tiles.Tile32);

        // Game phases and rules
        AddGamePhase("play")
            .WithEndGameDetection(
                game => new CheckersEndgameCondition(game),
                game => new CheckersEndGameMutator(game))
            .If<GameNotEndedCondition>()
            .Then()
                // Move piece (validates mandatory capture, handles king promotion and captures)
                .ForEvent<MovePieceGameEvent>()
                    .If<PieceIsActivePlayerGameEventCondition>()
                        .And(game => new KingMovementCondition(game))
                        .And(game => new MandatoryCaptureCondition(game))
                        .And<DestinationIsEmptyGameEventCondition>()
                .Then()
                    .Do<MovePieceStateMutator>()
                    .Do(game => new CheckersCapturePieceStateMutator(game))
                    .Do(game => new PromoteToKingMutator(game))
                    .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()));
    }

    /// <summary>
    /// Builds the dark-square only topology for standard 8x8 checkers board.
    /// Creates 32 tiles with diagonal connections following checkers numbering.
    /// </summary>
    private void BuildDarkSquareTopology()
    {
        // Standard checkers uses dark squares only in a specific numbering pattern
        // Row 1 (bottom): 1, 2, 3, 4 (on dark squares)
        // Row 2: 5, 6, 7, 8
        // ... Row 8 (top): 29, 30, 31, 32

        // Create all 32 tiles
        for (int i = 1; i <= 32; i++)
        {
            AddTile($"tile-{i}");
        }

        // Add diagonal connections based on checkers geometry
        // Each tile connects to up to 4 diagonal neighbors (NE, NW, SE, SW)
        
        // Helper to add bidirectional diagonal connection
        void AddDiagonalConnection(int fromTile, int toTile, string direction)
        {
            WithTile($"tile-{fromTile}").WithRelationTo($"tile-{toTile}").InDirection(direction);
        }

        // Row 1 connections (tiles 1-4)
        AddDiagonalConnection(1, 5, Constants.Directions.SouthWest);
        AddDiagonalConnection(1, 6, Constants.Directions.SouthEast);
        AddDiagonalConnection(2, 6, Constants.Directions.SouthWest);
        AddDiagonalConnection(2, 7, Constants.Directions.SouthEast);
        AddDiagonalConnection(3, 7, Constants.Directions.SouthWest);
        AddDiagonalConnection(3, 8, Constants.Directions.SouthEast);
        AddDiagonalConnection(4, 8, Constants.Directions.SouthWest);

        // Row 2 connections (tiles 5-8)
        AddDiagonalConnection(5, 1, Constants.Directions.NorthEast);
        AddDiagonalConnection(5, 9, Constants.Directions.SouthWest);
        AddDiagonalConnection(6, 1, Constants.Directions.NorthWest);
        AddDiagonalConnection(6, 2, Constants.Directions.NorthEast);
        AddDiagonalConnection(6, 9, Constants.Directions.SouthEast);
        AddDiagonalConnection(6, 10, Constants.Directions.SouthWest);
        AddDiagonalConnection(7, 2, Constants.Directions.NorthWest);
        AddDiagonalConnection(7, 3, Constants.Directions.NorthEast);
        AddDiagonalConnection(7, 10, Constants.Directions.SouthEast);
        AddDiagonalConnection(7, 11, Constants.Directions.SouthWest);
        AddDiagonalConnection(8, 3, Constants.Directions.NorthWest);
        AddDiagonalConnection(8, 4, Constants.Directions.NorthEast);
        AddDiagonalConnection(8, 11, Constants.Directions.SouthEast);
        AddDiagonalConnection(8, 12, Constants.Directions.SouthWest);

        // Row 3 connections (tiles 9-12)
        AddDiagonalConnection(9, 5, Constants.Directions.NorthEast);
        AddDiagonalConnection(9, 6, Constants.Directions.NorthWest);
        AddDiagonalConnection(9, 13, Constants.Directions.SouthWest);
        AddDiagonalConnection(9, 14, Constants.Directions.SouthEast);
        AddDiagonalConnection(10, 6, Constants.Directions.NorthEast);
        AddDiagonalConnection(10, 7, Constants.Directions.NorthWest);
        AddDiagonalConnection(10, 14, Constants.Directions.SouthWest);
        AddDiagonalConnection(10, 15, Constants.Directions.SouthEast);
        AddDiagonalConnection(11, 7, Constants.Directions.NorthEast);
        AddDiagonalConnection(11, 8, Constants.Directions.NorthWest);
        AddDiagonalConnection(11, 15, Constants.Directions.SouthWest);
        AddDiagonalConnection(11, 16, Constants.Directions.SouthEast);
        AddDiagonalConnection(12, 8, Constants.Directions.NorthEast);
        AddDiagonalConnection(12, 16, Constants.Directions.SouthWest);

        // Row 4 connections (tiles 13-16)
        AddDiagonalConnection(13, 9, Constants.Directions.NorthEast);
        AddDiagonalConnection(13, 17, Constants.Directions.SouthWest);
        AddDiagonalConnection(14, 9, Constants.Directions.NorthWest);
        AddDiagonalConnection(14, 10, Constants.Directions.NorthEast);
        AddDiagonalConnection(14, 17, Constants.Directions.SouthEast);
        AddDiagonalConnection(14, 18, Constants.Directions.SouthWest);
        AddDiagonalConnection(15, 10, Constants.Directions.NorthWest);
        AddDiagonalConnection(15, 11, Constants.Directions.NorthEast);
        AddDiagonalConnection(15, 18, Constants.Directions.SouthEast);
        AddDiagonalConnection(15, 19, Constants.Directions.SouthWest);
        AddDiagonalConnection(16, 11, Constants.Directions.NorthWest);
        AddDiagonalConnection(16, 12, Constants.Directions.NorthEast);
        AddDiagonalConnection(16, 19, Constants.Directions.SouthEast);
        AddDiagonalConnection(16, 20, Constants.Directions.SouthWest);

        // Row 5 connections (tiles 17-20)
        AddDiagonalConnection(17, 13, Constants.Directions.NorthEast);
        AddDiagonalConnection(17, 14, Constants.Directions.NorthWest);
        AddDiagonalConnection(17, 21, Constants.Directions.SouthWest);
        AddDiagonalConnection(17, 22, Constants.Directions.SouthEast);
        AddDiagonalConnection(18, 14, Constants.Directions.NorthEast);
        AddDiagonalConnection(18, 15, Constants.Directions.NorthWest);
        AddDiagonalConnection(18, 22, Constants.Directions.SouthWest);
        AddDiagonalConnection(18, 23, Constants.Directions.SouthEast);
        AddDiagonalConnection(19, 15, Constants.Directions.NorthEast);
        AddDiagonalConnection(19, 16, Constants.Directions.NorthWest);
        AddDiagonalConnection(19, 23, Constants.Directions.SouthWest);
        AddDiagonalConnection(19, 24, Constants.Directions.SouthEast);
        AddDiagonalConnection(20, 16, Constants.Directions.NorthEast);
        AddDiagonalConnection(20, 24, Constants.Directions.SouthWest);

        // Row 6 connections (tiles 21-24)
        AddDiagonalConnection(21, 17, Constants.Directions.NorthEast);
        AddDiagonalConnection(21, 25, Constants.Directions.SouthWest);
        AddDiagonalConnection(22, 17, Constants.Directions.NorthWest);
        AddDiagonalConnection(22, 18, Constants.Directions.NorthEast);
        AddDiagonalConnection(22, 25, Constants.Directions.SouthEast);
        AddDiagonalConnection(22, 26, Constants.Directions.SouthWest);
        AddDiagonalConnection(23, 18, Constants.Directions.NorthWest);
        AddDiagonalConnection(23, 19, Constants.Directions.NorthEast);
        AddDiagonalConnection(23, 26, Constants.Directions.SouthEast);
        AddDiagonalConnection(23, 27, Constants.Directions.SouthWest);
        AddDiagonalConnection(24, 19, Constants.Directions.NorthWest);
        AddDiagonalConnection(24, 20, Constants.Directions.NorthEast);
        AddDiagonalConnection(24, 27, Constants.Directions.SouthEast);
        AddDiagonalConnection(24, 28, Constants.Directions.SouthWest);

        // Row 7 connections (tiles 25-28)
        AddDiagonalConnection(25, 21, Constants.Directions.NorthEast);
        AddDiagonalConnection(25, 22, Constants.Directions.NorthWest);
        AddDiagonalConnection(25, 29, Constants.Directions.SouthWest);
        AddDiagonalConnection(25, 30, Constants.Directions.SouthEast);
        AddDiagonalConnection(26, 22, Constants.Directions.NorthEast);
        AddDiagonalConnection(26, 23, Constants.Directions.NorthWest);
        AddDiagonalConnection(26, 30, Constants.Directions.SouthWest);
        AddDiagonalConnection(26, 31, Constants.Directions.SouthEast);
        AddDiagonalConnection(27, 23, Constants.Directions.NorthEast);
        AddDiagonalConnection(27, 24, Constants.Directions.NorthWest);
        AddDiagonalConnection(27, 31, Constants.Directions.SouthWest);
        AddDiagonalConnection(27, 32, Constants.Directions.SouthEast);
        AddDiagonalConnection(28, 24, Constants.Directions.NorthEast);
        AddDiagonalConnection(28, 32, Constants.Directions.SouthWest);

        // Row 8 connections (tiles 29-32, only backward connections)
        AddDiagonalConnection(29, 25, Constants.Directions.NorthEast);
        AddDiagonalConnection(30, 25, Constants.Directions.NorthWest);
        AddDiagonalConnection(30, 26, Constants.Directions.NorthEast);
        AddDiagonalConnection(31, 26, Constants.Directions.NorthWest);
        AddDiagonalConnection(31, 27, Constants.Directions.NorthEast);
        AddDiagonalConnection(32, 27, Constants.Directions.NorthWest);
        AddDiagonalConnection(32, 28, Constants.Directions.NorthEast);
    }
}
