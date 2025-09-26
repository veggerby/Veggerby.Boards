using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

public class ChessPawnBasicTests
{
    [Fact]
    public void GivenInitialPosition_WhenWhitePawnMovesOne_ThenPawnAdvances()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        // act
        progress = progress.Move("white-pawn-5", "e3");
        // assert
        var pawn = progress.Game.GetPiece("white-pawn-5");
        progress.State.GetState<PieceState>(pawn).CurrentTile.Id.Should().Be("tile-e3");
    }

    [Fact]
    public void GivenInitialPosition_WhenWhitePawnAttemptsToMoveOntoOwnPiece_ThenIgnored()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        // act (attempt queen onto occupied e2)
        var queen = progress.Game.GetPiece("white-queen");
        var before = progress.State.GetState<PieceState>(queen).CurrentTile;
        progress = progress.Move("white-queen", "e2");
        // assert
        progress.State.GetState<PieceState>(queen).CurrentTile.Should().Be(before);
    }
}