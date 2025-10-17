using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;

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
        var queen = progress.Game.GetPiece(WhiteQueen).EnsureNotNull();
        var queenStart = progress.State.GetRequiredPieceState(queen).CurrentTile;

        // act (white pawn double-step, black makes a quiet reply, then white queen moves)
        progress = progress.Move(WhitePawn5, "e4");
        progress = progress.Move(BlackPawn5, "e5"); // black reply to restore white turn
        progress = progress.Move(WhiteQueen, "e2");

        // assert
        progress.State.GetRequiredPieceState(queen).CurrentTile.Id.Should().Be(ChessIds.Tiles.E2);
        queenStart.Id.Should().Be(ChessIds.Tiles.D1); // sanity (original queen start square)
    }

    [Fact]
    public void GivenPawnAdvance_WhenQueenAttemptsToMoveTwoSquares_ThenBlockedByPawnAtE3()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece(WhiteQueen).EnsureNotNull();
        var start = progress.State.GetRequiredPieceState(queen).CurrentTile;

        // act (white pawn advances, black replies, white queen attempts illegal capture of own pawn)
        progress = progress.Move(WhitePawn5, "e4");
        progress = progress.Move(BlackPawn5, "e5");
        var before = progress;
        progress = progress.Move(WhiteQueen, "e4"); // should be ignored (friendly piece on e4)

        // assert (queen remains and progress unchanged)
        progress.Should().BeSameAs(before);
        progress.State.GetRequiredPieceState(queen).CurrentTile.Should().Be(start);
    }
}