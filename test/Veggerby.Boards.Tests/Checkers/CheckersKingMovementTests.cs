using System.Linq;

using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersKingMovementTests
{
    [Fact]
    public void Should_promote_black_piece_when_reaching_row_8()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();
        
        // Move black piece from tile 1 toward promotion row (tile 29)
        // Path: 1 → 5 → 9 → 13 → 17 → 21 → 25 → 29
        
        // act & assert - Move step by step
        progress = progress.Move("black-piece-1", "tile-5");   // Row 1→2
        progress.Move("white-piece-1", "tile-17");             // White moves
        
        progress = progress.Move("black-piece-1", "tile-9");   // Row 2→3
        progress = progress.Move("white-piece-2", "tile-18");  // White moves
        
        progress = progress.Move("black-piece-1", "tile-13");  // Row 3→4
        progress = progress.Move("white-piece-3", "tile-19");  // White moves
        
        progress = progress.Move("black-piece-1", "tile-18");  // Row 4→5 (capture white piece 2 if implemented)
        progress = progress.Move("white-piece-4", "tile-20");  // White moves
        
        progress = progress.Move("black-piece-1", "tile-22");  // Row 5→6
        progress = progress.Move("white-piece-5", "tile-21");  // White moves
        
        progress = progress.Move("black-piece-1", "tile-26");  // Row 6→7
        progress = progress.Move("white-piece-6", "tile-22");  // White moves
        
        progress = progress.Move("black-piece-1", "tile-30");  // Row 7→8 PROMOTION!
        
        // assert - check if piece was promoted
        var promotedPieces = progress.State.GetStates<PromotedPieceState>().ToList();
        promotedPieces.Should().NotBeEmpty("piece should be promoted when reaching row 8");
        promotedPieces.Should().Contain(p => p.PromotedPiece.Id == "black-piece-1");
    }
}
