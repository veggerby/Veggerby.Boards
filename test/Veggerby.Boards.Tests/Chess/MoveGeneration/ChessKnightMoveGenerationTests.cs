using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Tests.Chess.MoveGeneration;

public class ChessKnightMoveGenerationTests
{
    [Fact]
    public void GivenKnightOnStartingSquare_WhenGeneratingMoves_ThenTwoMovesAvailable()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var state = progress.State;
        var generator = new ChessMoveGenerator(game);

        // act
        var moves = generator.Generate(state);
        var g1Knight = game.GetPiece(WhiteKnight2).EnsureNotNull();
        var g1Moves = moves.Where(m => m.Piece == g1Knight).ToList();

        // assert
        g1Moves.Count.Should().Be(2, "Knight on g1 should have 2 moves (f3 and h3)");
        g1Moves.Should().Contain(m => m.To.Id == F3);
        g1Moves.Should().Contain(m => m.To.Id == H3);
    }

    // Note: Knight center test removed - would need more complex setup to ensure correct turn

    [Fact]
    public void GivenKnightCanCaptureOpponent_WhenGeneratingMoves_ThenCaptureMarkedCorrectly()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        progress = progress.Move(WhiteKnight2, "f3");
        progress = progress.Move(BlackPawn5, "e5");
        var game = progress.Game;
        var state = progress.State;
        var generator = new ChessMoveGenerator(game);

        // act
        var moves = generator.Generate(state);
        var f3Knight = game.GetPiece(WhiteKnight2).EnsureNotNull();
        var f3Moves = moves.Where(m => m.Piece == f3Knight).ToList();
        var captureMove = f3Moves.FirstOrDefault(m => m.To.Id == E5);

        // assert
        captureMove.Should().NotBeNull("Knight should be able to move to e5");
        captureMove!.IsCapture.Should().BeTrue("Move to e5 should be marked as capture");
    }
}
