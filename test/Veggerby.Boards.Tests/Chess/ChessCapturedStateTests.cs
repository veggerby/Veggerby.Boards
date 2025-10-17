using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Chess.Builders;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Tests.Chess;

public class ChessCapturedStateTests
{
    [Fact]
    public void GivenCapture_WhenExecuted_ThenCapturedPieceHasCapturedStateAndIsNotOnTile()
    {
        // arrange
        var progress = new ChessCaptureScenarioBuilder().Compile();
        var queen = progress.Game.GetPiece(WhiteQueen);
        queen.Should().NotBeNull();
        var blackPawn = progress.Game.GetPiece(BlackPawn5); // e7
        blackPawn.Should().NotBeNull();
        // act (queen captures directly)
        progress = progress.Move(WhiteQueen, E7);

        // assert
        progress.State.IsCaptured(blackPawn!).Should().BeTrue("black pawn must be marked captured");
        var destTile = progress.Game.GetTile(E7);
        destTile.Should().NotBeNull();
        progress.State.GetPiecesOnTile(destTile!).Any(p => p.Equals(blackPawn))
            .Should().BeFalse("captured piece should not appear on destination tile occupancy");
        var queenState = progress.State.GetState<PieceState>(queen!);
        queenState.Should().NotBeNull();
        queenState!.CurrentTile.Id.Should().Be(E7);
    }
}