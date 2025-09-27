using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

public class ChessCapturedStateTests
{
    [Fact(Skip = "Deferred until extended pawn movement / setup helpers allow clearing full file for multi-step capture scenario.")]
    public void GivenCapture_WhenExecuted_ThenCapturedPieceHasCapturedStateAndIsNotOnTile()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece("white-queen");
        var blackPawn = progress.Game.GetPiece("black-pawn-5"); // e7

        // clear path (move white pawn forward leaving e7 occupied by black pawn)
        progress = progress.Move("white-pawn-5", "e3");
        progress = progress.Move("white-pawn-5", "e4"); // stop here: leave e5,e6 empty for queen path

        // act (queen captures)
        progress = progress.Move("white-queen", "e2");
        progress = progress.Move("white-queen", "e3");
        progress = progress.Move("white-queen", "e4");
        progress = progress.Move("white-queen", "e5");
        progress = progress.Move("white-queen", "e6"); // empty square
        progress = progress.Move("white-queen", "e7");

        // assert
        progress.State.IsCaptured(blackPawn).Should().BeTrue("black pawn must be marked captured");
        var destTile = progress.Game.GetTile("tile-e7");
        progress.State.GetPiecesOnTile(destTile).Any(p => p.Equals(blackPawn))
            .Should().BeFalse("captured piece should not appear on destination tile occupancy");
        var queenState = progress.State.GetState<PieceState>(queen);
        queenState.CurrentTile.Id.Should().Be("tile-e7");
    }
}