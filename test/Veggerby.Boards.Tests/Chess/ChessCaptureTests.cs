using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

public class ChessCaptureTests
{
    [Fact(Skip = "Deferred until extended pawn movement / setup helpers allow clearing full file for multi-step capture scenario.")]
    public void GivenClearedFile_WhenQueenMovesOntoBlackPawn_ThenPawnCapturedAndQueenOccupiesTile()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece("white-queen");
        var blackPawn = progress.Game.GetPiece("black-pawn-5"); // on e7

        // clear path: move white pawn off e-file upward (south direction is toward black in current coordinate system?)
        // In builder: white pawns have direction south; so move white-pawn-5 stepwise to free e2..e6
        progress = progress.Move("white-pawn-5", "e3");
        progress = progress.Move("white-pawn-5", "e4"); // stop here: leave e5 and e6 empty so queen can traverse to e7

        // act: move queen along now-cleared file to original black pawn square (which may be vacated if earlier capture not implemented yet)
        progress = progress.Move("white-queen", "e2");
        progress = progress.Move("white-queen", "e3");
        progress = progress.Move("white-queen", "e4");
        progress = progress.Move("white-queen", "e5");
        // e6 now empty (pawn stopped at e5)
        progress = progress.Move("white-queen", "e6");
        progress = progress.Move("white-queen", "e7"); // queen captures black pawn on arrival

        // assert
        var queenState = progress.State.GetState<PieceState>(queen);
        queenState.CurrentTile.Id.Should().Be("tile-e7");

        // black pawn should now have a captured state and not appear on e7
        progress.State.IsCaptured(blackPawn).Should().BeTrue("black pawn should be marked captured");
        progress.State.GetPiecesOnTile(queenState.CurrentTile)
            .Any(p => p.Equals(blackPawn))
            .Should().BeFalse("captured pawn must not occupy destination tile");
    }
}