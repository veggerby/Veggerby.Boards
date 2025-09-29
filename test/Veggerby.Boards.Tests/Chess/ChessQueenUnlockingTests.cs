using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Additional coverage for queen movement legality after clearing blocking pieces.
/// Ensures conditions transition from Ignore to Valid when path becomes unobstructed and destination is free.
/// </summary>
public class ChessQueenUnlockingTests
{
    [Fact]
    public void GivenPawnAdvance_WhenQueenMovesIntoVacatedSquare_ThenQueenMoves()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece("white-queen");
        var queenStart = progress.State.GetState<PieceState>(queen).CurrentTile;

        // act (white pawn double-step, black makes a quiet reply, then white queen moves)
        progress = progress.Move("white-pawn-5", "e4");
        progress = progress.Move("black-pawn-5", "e5"); // black reply to restore white turn
        progress = progress.Move("white-queen", "e2");

        // assert
        progress.State.GetState<PieceState>(queen).CurrentTile.Id.Should().Be("tile-e2");
        queenStart.Id.Should().Be("tile-d1"); // sanity (original queen start square)
    }

    [Fact]
    public void GivenPawnAdvance_WhenQueenAttemptsToMoveTwoSquares_ThenBlockedByPawnAtE3()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece("white-queen");
        var start = progress.State.GetState<PieceState>(queen).CurrentTile;

        // act (white pawn advances, black replies, white queen attempts illegal capture of own pawn)
        progress = progress.Move("white-pawn-5", "e4");
        progress = progress.Move("black-pawn-5", "e5");
        var before = progress;
        progress = progress.Move("white-queen", "e4"); // should be ignored (friendly piece on e4)

        // assert (queen remains and progress unchanged)
        progress.Should().BeSameAs(before);
        progress.State.GetState<PieceState>(queen).CurrentTile.Should().Be(start);
    }
}