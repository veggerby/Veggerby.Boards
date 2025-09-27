using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Scaffolding tests for future chess capture semantics. Marked Skip until capture rules & mutators are implemented.
/// </summary>
public class ChessCaptureScaffoldingTests
{
    [Fact(Skip = "Capture semantics not implemented yet")]
    public void GivenOpponentPieceInLine_WhenQueenMovesOntoIt_ThenCaptureOccurs()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var whiteQueen = progress.Game.GetPiece("white-queen");
        var blackPawn = progress.Game.GetPiece("black-pawn-5"); // on e7 initially

        // act (hypothetical future sequence: clear path, then capture)
        progress = progress.Move("white-pawn-5", "e3"); // free e2
        progress = progress.Move("white-pawn-5", "e4"); // future double-move logic might differ
        progress = progress.Move("white-pawn-5", "e5"); // assume incremental moves for now
        progress = progress.Move("white-pawn-5", "e6");
        progress = progress.Move("white-pawn-5", "e7"); // would attempt capture in future
        // assert (future): black pawn removed; queen unchanged; or if queen moves to capture scenario etc.
        progress.State.GetState<PieceState>(whiteQueen).CurrentTile.Id.Should().Be("tile-e1");
        progress.State.GetState<PieceState>(blackPawn).CurrentTile.Id.Should().Be("tile-e7");
    }
}