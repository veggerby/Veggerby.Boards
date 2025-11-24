using System.Linq;

using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersJumpCaptureDebugTests
{
    [Fact]
    public void Debug_Jump_Move_Should_Capture_Piece()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // Set up a simple capture scenario
        // Move black piece from 9 to 13
        progress = progress.Move("black-piece-9", "tile-13");

        // Move white piece from 21 to 17 (now adjacent to black piece on 13)
        progress = progress.Move("white-piece-1", "tile-17");

        // Verify pieces are in position
        var blackOn13 = progress.State.GetPiecesOnTile(progress.Game.Board.GetTile("tile-13")!).FirstOrDefault();
        var whiteOn17 = progress.State.GetPiecesOnTile(progress.Game.Board.GetTile("tile-17")!).FirstOrDefault();

        blackOn13.Should().NotBeNull();
        whiteOn17.Should().NotBeNull();

        // Count pieces before capture
        var blackCountBefore = progress.State.GetStates<PieceState>()
            .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
        var whiteCountBefore = progress.State.GetStates<PieceState>()
            .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.White);
        var capturedBefore = progress.State.GetStates<CapturedPieceState>().Count();

        // act - Now white jumps from 21 to 13, capturing black piece on 17
        // Wait, white piece is already on 17, not 21. Let me restart...

        // Let me redo the setup more carefully
        progress = new CheckersGameBuilder().Compile();

        // Black moves 9→13
        progress = progress.Move("black-piece-9", "tile-13");

        // White moves 21→17 (piece-1)
        progress = progress.Move("white-piece-1", "tile-17");

        // Now black should be able to jump: 13 + SE + SE = 13→17→21
        // But tile 21 might have white piece-5 on it initially
        // Let me check what's on tile-21

        // If there's a white piece on 21, black can't jump there
        // Let me instead have white jump over black

        //act - White piece on 21 (piece-5) jumps over black on 17 to land on 13
        // Wait, I moved white-1 to 17, not piece-5

        // Let me restart with a clearer setup
        progress = new CheckersGameBuilder().Compile();

        // Move black 9→14 (SE direction)
        progress = progress.Move("black-piece-9", "tile-14");

        // Move white 22→18
        progress = progress.Move("white-piece-2", "tile-18");

        // Move black 10→15
        progress = progress.Move("black-piece-10", "tile-15");

        // Now white can jump: 18 over 14 to 10
        // Check path: 18 + NW = 14, 14 + NW = 10

        blackCountBefore = progress.State.GetStates<PieceState>()
            .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
        capturedBefore = progress.State.GetStates<CapturedPieceState>().Count();

        // White jumps from 18 over 14 to 10
        progress = progress.Move("white-piece-2", "tile-10");

        // assert
        var blackCountAfter = progress.State.GetStates<PieceState>()
            .Count(ps => ps.Artifact.Owner.Id == CheckersIds.Players.Black);
        var capturedAfter = progress.State.GetStates<CapturedPieceState>().Count();

        // Black count should decrease by 1
        blackCountAfter.Should().Be(blackCountBefore - 1, "one black piece should be captured");
        capturedAfter.Should().Be(1, "one piece should be in captured state");
    }
}
