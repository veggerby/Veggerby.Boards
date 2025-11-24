using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersPromotionRealTest
{
    [Fact]
    public void White_piece_can_promote_to_king_on_tile_1()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // act - move white-piece-5 from tile-21 to tile-1 (promotion row)
        // Clear path by moving pieces aside
        
        progress = progress.Move("black-piece-9", "tile-13");   // Black 9→13
        progress = progress.Move("white-piece-5", "tile-17");   // White 5: 21→17
        
        progress = progress.Move("black-piece-10", "tile-14");  // Black 10→14
        progress = progress.Move("white-piece-5", "tile-13");   // White 5: 17→13
        
        progress = progress.Move("black-piece-11", "tile-15");  // Black 11→15
        progress = progress.Move("white-piece-5", "tile-9");    // White 5: 13→9
        
        progress = progress.Move("black-piece-12", "tile-16");  // Black 12→16
        progress = progress.Move("white-piece-5", "tile-5");    // White 5: 9→5
        
        progress = progress.Move("black-piece-1", "tile-6");    // Black 1→6 (move aside)
        progress = progress.Move("white-piece-5", "tile-1");    // White 5: 5→1 - PROMOTION!

        // assert
        var whitePiece5 = progress.Game.GetPiece("white-piece-5");
        var piece5State = progress.State.GetState<PieceState>(whitePiece5!);
        
        // Verify piece reached tile-1
        piece5State!.CurrentTile.Id.Should().Be("tile-1");

        // Verify piece was promoted
        var promotedStates = progress.State.GetStates<PromotedPieceState>().ToList();
        promotedStates.Should().HaveCount(1);
        promotedStates.First().PromotedPiece.Id.Should().Be("white-piece-5");
    }
}
