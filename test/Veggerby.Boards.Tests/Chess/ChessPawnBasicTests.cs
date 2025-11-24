using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.Constants.ChessIds.Pieces;

using Veggerby.Boards.Chess.Extensions;
namespace Veggerby.Boards.Tests.Chess;

public class ChessPawnBasicTests
{
    [Fact]
    public void GivenInitialPosition_WhenWhitePawnMovesOne_ThenPawnAdvances()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        // act
        progress = progress.Move(WhitePawn5, "e3");
        // assert
        var pawn = progress.Game.GetPiece(WhitePawn5).EnsureNotNull();
        progress.State.GetRequiredPieceState(pawn).CurrentTile.Id.Should().Be(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E3);
    }

    [Fact]
    public void GivenInitialPosition_WhenWhitePawnAttemptsToMoveOntoOwnPiece_ThenIgnored()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        // act (attempt queen onto occupied e2)
        var queen = progress.Game.GetPiece(WhiteQueen).EnsureNotNull();
        var before = progress.State.GetRequiredPieceState(queen).CurrentTile;
        progress = progress.Move(WhiteQueen, "e2");
        // assert
        progress.State.GetRequiredPieceState(queen).CurrentTile.Should().Be(before);
    }
}
