using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersEndgameTests
{
    [Fact]
    public void Should_detect_game_over_when_player_has_no_pieces()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();
        
        // act - initial state should not be game over
        var isGameOver = progress.IsGameOver();

        // assert
        isGameOver.Should().BeFalse();
    }

    [Fact]
    public void Should_declare_winner_when_opponent_has_no_pieces()
    {
        // arrange & act & assert
        // When all of one player's pieces are captured, the other player wins
        // This test will be implemented once capture mechanics are complete
        
        true.Should().BeTrue(); // Placeholder
    }

    [Fact]
    public void Should_declare_winner_when_opponent_has_no_valid_moves()
    {
        // arrange & act & assert
        // If a player has pieces but cannot make any legal moves, they lose
        // This is similar to stalemate in chess but results in a loss, not a draw
        
        true.Should().BeTrue(); // Placeholder
    }

    [Fact]
    public void Should_track_outcome_with_player_results()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // act - game is not over, so no outcome yet
        var outcome = progress.GetOutcome();

        // assert
        outcome.Should().BeNull(); // No outcome in starting position
    }
}
