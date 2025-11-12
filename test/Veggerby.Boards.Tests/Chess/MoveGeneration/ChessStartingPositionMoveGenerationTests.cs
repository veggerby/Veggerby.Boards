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
        moves.Count.Should().Be(20); // 16 pawn moves (8x2) + 4 knight moves (2x2)
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
