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

        // act (move blocking pawn one step forward, then move queen into e2)
        progress = progress.Move("white-pawn-5", "e3"); // frees e2
        progress = progress.Move("white-queen", "e2");

        // assert
        progress.State.GetState<PieceState>(queen).CurrentTile.Id.Should().Be("tile-e2");
        queenStart.Id.Should().Be("tile-e1"); // sanity (original)
    }

    [Fact]
    public void GivenPawnAdvance_WhenQueenAttemptsToMoveTwoSquares_ThenBlockedByPawnAtE3()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece("white-queen");
        var start = progress.State.GetState<PieceState>(queen).CurrentTile;

        // act
        progress = progress.Move("white-pawn-5", "e3"); // pawn now on e3 leaving e2 empty
        progress = progress.Move("white-queen", "e3"); // destination occupied by friendly pawn

        // assert (queen should remain on original square e1)
        progress.State.GetState<PieceState>(queen).CurrentTile.Should().Be(start);
    }
}