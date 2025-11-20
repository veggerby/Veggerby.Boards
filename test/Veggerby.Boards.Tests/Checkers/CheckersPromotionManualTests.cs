using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersPromotionManualTests
{
    [Fact]
    public void Should_promote_black_piece_when_moving_to_tile_29()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();
        
        // We need to manually move a piece step by step toward tile 29
        // Black piece 9 starts on tile 9
        // Path: 9 → 13 → 17 → 21 → 25 → 29
        
        // act
        progress = progress.Move("black-piece-9", "tile-13");  // Row 3→4
        progress = progress.Move("white-piece-5", "tile-21");  // White moves
        progress = progress.Move("black-piece-9", "tile-17");  // Row 4→5
        progress = progress.Move("white-piece-6", "tile-22");  // White moves
        progress = progress.Move("black-piece-9", "tile-21");  // Row 5→6
        progress = progress.Move("white-piece-7", "tile-23");  // White moves
        progress = progress.Move("black-piece-9", "tile-25");  // Row 6→7
        progress = progress.Move("white-piece-8", "tile-24");  // White moves
        progress = progress.Move("black-piece-9", "tile-29");  // Row 7→8 PROMOTION!
        
        // assert
        var piece9State = progress.State.GetStates<PieceState>()
            .FirstOrDefault(ps => ps.Artifact.Id == "black-piece-9");
        
        // First verify the piece actually moved to tile 29
        piece9State.Should().NotBeNull();
        piece9State!.CurrentTile.Id.Should().Be("tile-29", "piece should have moved to the promotion tile");
        
        var promotedPieces = progress.State.GetStates<PromotedPieceState>().ToList();
        
        promotedPieces.Should().NotBeEmpty("piece should be promoted when reaching row 8");
        promotedPieces.Should().HaveCount(1);
        promotedPieces[0].PromotedPiece.Id.Should().Be("black-piece-9");
    }
}
