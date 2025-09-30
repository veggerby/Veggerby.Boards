using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Chess.Builders;

namespace Veggerby.Boards.Tests.Chess;

public class ChessCapturedStateTests
{
    [Fact]
    public void GivenCapture_WhenExecuted_ThenCapturedPieceHasCapturedStateAndIsNotOnTile()
    {
        // arrange
        var progress = new ChessCaptureScenarioBuilder().Compile();
        var queen = progress.Game.GetPiece("white-queen");
        var blackPawn = progress.Game.GetPiece("black-pawn-5"); // e7
        // act (queen captures directly)
        progress = progress.Move("white-queen", "e7");

        // assert
        progress.State.IsCaptured(blackPawn).Should().BeTrue("black pawn must be marked captured");
        var destTile = progress.Game.GetTile(ChessIds.Tiles.E7);
        progress.State.GetPiecesOnTile(destTile).Any(p => p.Equals(blackPawn))
            .Should().BeFalse("captured piece should not appear on destination tile occupancy");
        var queenState = progress.State.GetState<PieceState>(queen);
        queenState.CurrentTile.Id.Should().Be(ChessIds.Tiles.E7);
    }
}