using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Chess.MoveGeneration;

public class ChessStartingPositionMoveGenerationTests
{
    [Fact]
    public void GivenStartingPosition_WhenGeneratingPseudoLegalMoves_ThenTwentyMovesReturned()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var state = progress.State;

        // act
        var generator = new ChessMoveGenerator(game);
        var moves = generator.Generate(state);

        // assert
        // Expected: 16 pawn moves (8 pawns × 2 moves each) + 4 knight moves (2 knights × 2 moves each) = 20
        moves.Count.Should().Be(20, "Starting position should have exactly 20 legal moves");
    }

    [Fact]
    public void GivenStartingPosition_WhenGeneratingMoves_ThenWhiteHasOnlyPawnAndKnightMoves()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var state = progress.State;

        // act
        var generator = new ChessMoveGenerator(game);
        var moves = generator.Generate(state);

        // assert
        moves.Should().OnlyContain(m =>
            ChessPiece.IsPawn(state, m.Piece.Id) ||
            ChessPiece.IsKnight(state, m.Piece.Id));
    }
}
