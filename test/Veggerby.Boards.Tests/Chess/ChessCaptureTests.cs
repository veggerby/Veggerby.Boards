using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Chess.Builders;

namespace Veggerby.Boards.Tests.Chess;

public class ChessCaptureTests
{
    [Fact]
    public void GivenClearedFile_WhenQueenMovesOntoBlackPawn_ThenPawnCapturedAndQueenOccupiesTile()
    {
        // arrange
        var progress = new ChessCaptureScenarioBuilder().Compile();
        var queen = progress.Game.GetPiece("white-queen");
        var blackPawn = progress.Game.GetPiece("black-pawn-5"); // on e7
        // act: queen traverses directly (multi-step path built internally by Move helper) to capture square
        progress = progress.Move("white-queen", "e7");

        // assert
        var queenState = progress.State.GetState<PieceState>(queen);
        queenState.CurrentTile.Id.Should().Be(ChessIds.Tiles.E7);

        // black pawn should now have a captured state and not appear on e7
        progress.State.IsCaptured(blackPawn).Should().BeTrue("black pawn should be marked captured");
        progress.State.GetPiecesOnTile(queenState.CurrentTile)
            .Any(p => p.Equals(blackPawn))
            .Should().BeFalse("captured pawn must not occupy destination tile");
    }
}