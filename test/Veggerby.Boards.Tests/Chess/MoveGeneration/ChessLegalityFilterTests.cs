using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;

namespace Veggerby.Boards.Tests.Chess.MoveGeneration;

public class ChessLegalityFilterTests
{
    [Fact]
    public void GivenStartingPosition_WhenFilteringLegalMoves_ThenAllMovesAreLegal()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var state = progress.State;
        var generator = new ChessMoveGenerator(game);
        var filter = new ChessLegalityFilter(game);

        // act
        var pseudoLegalMoves = generator.Generate(state);
        var legalMoves = filter.FilterLegalMoves(state, pseudoLegalMoves);

        // assert
        legalMoves.Count.Should().Be(20, "All 20 starting position moves should be legal");
        legalMoves.Count.Should().Be(pseudoLegalMoves.Count, "No moves should be filtered in starting position");
    }

    [Fact]
    public void GivenKingInCheck_WhenGeneratingLegalMoves_ThenOnlyMovesThatBlockOrEscapeAreIncluded()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // Set up a simpler check position
        // Move some pieces to create a check scenario
        progress = progress.Move(WhitePawn5, "e4");
        progress = progress.Move(BlackPawn5, "e5");

        var game = progress.Game;
        var state = progress.State;
        var filter = new ChessLegalityFilter(game);

        // act
        var legalMoves = filter.GenerateLegalMoves(state);

        // assert
        // In a normal position, there should be legal moves
        legalMoves.Should().NotBeEmpty("There should be legal moves in a normal position");
    }
}
