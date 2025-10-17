using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Chess.Support;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Tests ensuring active player alternation is enforced for chess.
/// </summary>
public class ChessTurnAlternationTests
{
    [Fact]
    public void GivenWhiteMoves_WhenWhiteAttemptsSecondConsecutiveMove_ThenIgnored()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var pawn = progress.Game.GetPiece(WhitePawn5).EnsureNotNull();
        var beforeBlackPawn = progress.Game.GetPiece(BlackPawn5).EnsureNotNull();
        // act1 white e2->e3
        progress = progress.Move(WhitePawn5, E3);
        var afterWhiteMove = progress;
        // act2 attempt another white move (same pawn e3->e4)
        progress = progress.Move(WhitePawn5, E4);
        // assert: ignored, pawn remains e3, state reference unchanged
        var pawnState = progress.State.GetState<PieceState>(pawn).EnsureNotNull();
        pawnState.CurrentTile.Id.Should().Be(ChessIds.Tiles.E3);
        progress.Should().BeSameAs(afterWhiteMove);
        // black still to move (can move now)
        progress = progress.Move(BlackPawn5, E6);
        progress.State.GetState<PieceState>(beforeBlackPawn).EnsureNotNull().CurrentTile.Id.Should().Be(ChessIds.Tiles.E6);
    }

    [Fact]
    public void GivenEnPassantTargetSet_WhenOpponentDoesNotCaptureImmediately_ThenCaptureLaterIgnored()
    {
        // arrange (white pawn e2, black pawn d4 ready; optional aux black pawn ensures reply)
        var progress = new EnPassantScenarioBuilder(includeAuxiliaryBlackPawn: true).Compile();
        var whitePawn = progress.Game.GetPiece("white-pawn-test").EnsureNotNull();
        var blackPawn = progress.Game.GetPiece("black-pawn-test").EnsureNotNull();
        // act1 white double-step e2->e4 sets target e3
        progress = progress.Move("white-pawn-test", "e4");
        // act2 black quiet move (aux pawn h7->h6) declines immediate capture
        progress = progress.Move("black-pawn-aux", "h6");
        var stateAfterDecline = progress;
        // act3 attempt delayed en-passant d4xe3 (should now be ignored since target cleared)
        progress = progress.Move("black-pawn-test", "e3");
        // assert: black pawn remained on d4, state unchanged
        var blackPawnState = progress.State.GetState<PieceState>(blackPawn).EnsureNotNull();
        blackPawnState.CurrentTile.Id.Should().Be(ChessIds.Tiles.D4);
        progress.Should().BeSameAs(stateAfterDecline);
        // white pawn still on e4
        progress.State.GetState<PieceState>(whitePawn).EnsureNotNull().CurrentTile.Id.Should().Be(ChessIds.Tiles.E4);
    }
}