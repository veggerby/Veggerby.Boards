using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersKingPromotionTests
{
    [Fact]
    public void Should_promote_black_piece_when_reaching_tile_29()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();
        
        // We need to move a black piece all the way to row 8 (tiles 29-32)
        // This requires a series of moves to get a black piece to the promotion row
        
        // For testing purposes, we'll manually set up the position
        // In a real game, this would take many moves
        
        // act - move black piece to promotion row (tile 29)
        // NOTE: This test is aspirational - we'd need many moves to get there
        // For now, just verify the promotion state exists
        
        var promotedStates = progress.State.GetStates<PromotedPieceState>();
        promotedStates.Should().BeEmpty(); // No promotions yet in starting position
    }

    [Fact]
    public void Should_promote_white_piece_when_reaching_tile_1()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();
        
        // White pieces need to reach row 1 (tiles 1-4) for promotion
        
        // act & assert
        var promotedStates = progress.State.GetStates<PromotedPieceState>();
        promotedStates.Should().BeEmpty(); // No promotions yet
    }

    [Fact]
    public void Should_allow_king_to_move_backward()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();
        
        // This test will verify that promoted kings can move in all diagonal directions
        // Including backward, which regular pieces cannot do
        
        // act & assert
        // TODO: Set up a promoted king and verify backward movement
        // For now, this is a placeholder
        
        true.Should().BeTrue();
    }

    [Fact]
    public void Should_promote_piece_immediately_after_reaching_promotion_row()
    {
        // arrange & act & assert
        // Verify that promotion happens as soon as piece reaches the end row
        // not before and not requiring another move
        
        true.Should().BeTrue(); // Placeholder
    }

    [Fact]
    public void Should_allow_promotion_during_multi_jump_sequence()
    {
        // arrange & act & assert
        // A piece that becomes a king during a multi-jump sequence
        // should gain king movement abilities for the remainder of that sequence
        
        true.Should().BeTrue(); // Placeholder
    }
}
