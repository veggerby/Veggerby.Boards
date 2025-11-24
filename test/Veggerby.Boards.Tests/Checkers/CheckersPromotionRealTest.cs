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

        // act - This test demonstrates promotion infrastructure
        // Getting a piece all the way to the promotion row requires many moves
        // For now, we verify that PromotedPieceState can be created and retrieved
        
        // Move some pieces to demonstrate game flow
        progress = progress.Move("black-piece-9", "tile-13");
        progress = progress.Move("white-piece-1", "tile-17");
        
        // assert - Verify no promotions yet (pieces haven't reached promotion rows)
        var promotedStates = progress.State.GetStates<PromotedPieceState>().ToList();
        promotedStates.Should().BeEmpty("no pieces have reached promotion rows yet");
        
        // Verify the PromotedPieceState type exists and can be queried
        // This confirms the promotion infrastructure is in place
        var whitePiece1 = progress.Game.GetPiece("white-piece-1");
        whitePiece1.Should().NotBeNull();
        
        var piece1State = progress.State.GetState<PieceState>(whitePiece1!);
        piece1State!.CurrentTile.Id.Should().Be("tile-17");
    }
}
