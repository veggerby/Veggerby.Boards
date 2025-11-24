using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;
using Xunit;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersPromotionIntegrationTest
{
    [Fact]
    public void Complete_game_with_white_piece_promotion_to_king()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // act - move white piece from tile-21 all the way to tile-1 for promotion
        // White piece-5 starts on tile-21

        // Clear a path for white piece-5 to reach tile-1
        // Move black pieces out of the way first
        progress = progress.Move("black-piece-9", "tile-13");  // Black 9→13
        progress = progress.Move("white-piece-5", "tile-17");  // White 21→17
        progress = progress.Move("black-piece-10", "tile-14"); // Black 10→14
        progress = progress.Move("white-piece-5", "tile-13");  // White 17→13 (moving toward promotion)
        progress = progress.Move("black-piece-11", "tile-15"); // Black 11→15
        progress = progress.Move("white-piece-5", "tile-9");   // White 13→9
        progress = progress.Move("black-piece-12", "tile-16"); // Black 12→16
        progress = progress.Move("white-piece-5", "tile-5");   // White 9→5
        progress = progress.Move("black-piece-1", "tile-6");   // Black 1→6
        progress = progress.Move("white-piece-5", "tile-1");   // White 5→1 - PROMOTION!

        // assert
        // Check that white-piece-5 is now on tile-1
        var whitePiece5 = progress.State.GetStates<PieceState>()
            .Single(x => x.Artifact.Id == "white-piece-5");
        whitePiece5.CurrentTile.Id.Should().Be("tile-1");

        // Check that piece was promoted
        var promotedStates = progress.State.GetStates<PromotedPieceState>().ToList();
        promotedStates.Should().HaveCount(1);
        promotedStates.First().PromotedPiece.Id.Should().Be("white-piece-5");

        // Verify the piece is now considered a king
        var promoted = promotedStates.First();
        promoted.Should().NotBeNull();
    }
}
