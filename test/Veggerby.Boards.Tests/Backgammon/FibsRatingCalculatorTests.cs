using Veggerby.Boards.Backgammon;

namespace Veggerby.Boards.Tests.Backgammon;

public class FibsRatingCalculatorTests
{
    [Fact]
    public void Should_calculate_underdog_win_probability_for_equal_ratings()
    {
        // arrange
        var ratingDifference = 0.0;
        var sqrtMatchLength = Math.Sqrt(1);

        // act
        var actual = FibsRatingCalculator.CalculateUnderdogWinProbability(ratingDifference, sqrtMatchLength);

        // assert
        actual.Should().BeApproximately(0.5, 0.0001);
    }

    [Fact]
    public void Should_calculate_underdog_win_probability_for_large_differential()
    {
        // arrange
        var ratingDifference = 400.0;
        var sqrtMatchLength = Math.Sqrt(1);

        // act
        var actual = FibsRatingCalculator.CalculateUnderdogWinProbability(ratingDifference, sqrtMatchLength);

        // assert
        actual.Should().BeLessThan(0.5);
        actual.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void Should_calculate_experience_factor_for_new_player()
    {
        // arrange
        var experience = 0;
        var matchLength = 1;

        // act
        var actual = FibsRatingCalculator.CalculateExperienceFactor(experience, matchLength);

        // assert
        actual.Should().BeApproximately(4.99, 0.01);
    }

    [Fact]
    public void Should_calculate_experience_factor_at_threshold()
    {
        // arrange
        var experience = 399;
        var matchLength = 1;

        // act
        var actual = FibsRatingCalculator.CalculateExperienceFactor(experience, matchLength);

        // assert
        actual.Should().BeApproximately(1.0, 0.01);
    }

    [Fact]
    public void Should_calculate_experience_factor_minimum_for_experienced_player()
    {
        // arrange
        var experience = 500;
        var matchLength = 1;

        // act
        var actual = FibsRatingCalculator.CalculateExperienceFactor(experience, matchLength);

        // assert
        actual.Should().Be(1.0);
    }

    [Fact]
    public void Should_calculate_rating_changes_for_equal_players_single_point_match()
    {
        // arrange
        var winnerRating = 1500.0;
        var winnerExperience = 0;
        var loserRating = 1500.0;
        var loserExperience = 0;
        var matchLength = 1;

        // act
        var (winner, loser) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        winner.RatingChange.Should().BeApproximately(9.98, 0.01);
        winner.NewRating.Should().BeApproximately(1509.98, 0.01);
        winner.NewExperience.Should().Be(1);

        loser.RatingChange.Should().BeApproximately(-9.98, 0.01);
        loser.NewRating.Should().BeApproximately(1490.02, 0.01);
        loser.NewExperience.Should().Be(1);
    }

    [Fact]
    public void Should_calculate_rating_changes_for_higher_rated_winner()
    {
        // arrange
        var winnerRating = 1600.0;
        var winnerExperience = 0;
        var loserRating = 1500.0;
        var loserExperience = 0;
        var matchLength = 1;

        // act
        var (winner, loser) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        winner.RatingChange.Should().BeGreaterThan(0.0);
        winner.RatingChange.Should().BeLessThan(10.0);

        loser.RatingChange.Should().BeLessThan(0.0);
        loser.RatingChange.Should().BeGreaterThan(-11.0);
    }

    [Fact]
    public void Should_calculate_rating_changes_for_lower_rated_winner_upset()
    {
        // arrange
        var winnerRating = 1400.0;
        var winnerExperience = 0;
        var loserRating = 1600.0;
        var loserExperience = 0;
        var matchLength = 1;

        // act
        var (winner, loser) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        winner.RatingChange.Should().BeGreaterThan(10.0);

        loser.RatingChange.Should().BeLessThan(0.0);
        loser.RatingChange.Should().BeGreaterThan(-10.0);
    }

    [Fact]
    public void Should_calculate_rating_changes_for_longer_match()
    {
        // arrange
        var winnerRating = 1500.0;
        var winnerExperience = 0;
        var loserRating = 1500.0;
        var loserExperience = 0;
        var matchLength = 9;

        // act
        var (winner, loser) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        winner.RatingChange.Should().BeApproximately(29.46, 0.01);
        winner.NewRating.Should().BeApproximately(1529.46, 0.01);
        winner.NewExperience.Should().Be(9);

        loser.RatingChange.Should().BeApproximately(-29.46, 0.01);
        loser.NewRating.Should().BeApproximately(1470.54, 0.01);
        loser.NewExperience.Should().Be(9);
    }

    [Fact]
    public void Should_calculate_rating_changes_with_experienced_players()
    {
        // arrange
        var winnerRating = 1500.0;
        var winnerExperience = 500;
        var loserRating = 1500.0;
        var loserExperience = 500;
        var matchLength = 1;

        // act
        var (winner, loser) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        winner.RatingChange.Should().BeApproximately(2.0, 0.01);
        winner.NewRating.Should().BeApproximately(1502.0, 0.01);
        winner.NewExperience.Should().Be(501);

        loser.RatingChange.Should().BeApproximately(-2.0, 0.01);
        loser.NewRating.Should().BeApproximately(1498.0, 0.01);
        loser.NewExperience.Should().Be(501);
    }

    [Fact]
    public void Should_throw_for_zero_match_length()
    {
        // arrange
        var winnerRating = 1500.0;
        var winnerExperience = 0;
        var loserRating = 1500.0;
        var loserExperience = 0;
        var matchLength = 0;

        // act
        var act = () => FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_calculate_rating_changes_for_13_point_match()
    {
        // arrange
        var winnerRating = 1550.0;
        var winnerExperience = 200;
        var loserRating = 1450.0;
        var loserExperience = 150;
        var matchLength = 13;

        // act
        var (winner, loser) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        winner.RatingChange.Should().BeGreaterThan(0.0);
        winner.NewExperience.Should().Be(213);

        loser.RatingChange.Should().BeLessThan(0.0);
        loser.NewExperience.Should().Be(163);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(9)]
    [InlineData(11)]
    [InlineData(13)]
    public void Should_calculate_rating_changes_for_common_match_lengths(int matchLength)
    {
        // arrange
        var winnerRating = 1500.0;
        var winnerExperience = 100;
        var loserRating = 1500.0;
        var loserExperience = 100;

        // act
        var (winner, loser) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRating, winnerExperience, loserRating, loserExperience, matchLength);

        // assert
        winner.RatingChange.Should().BeGreaterThan(0.0);
        winner.NewExperience.Should().Be(100 + matchLength);

        loser.RatingChange.Should().BeLessThan(0.0);
        loser.NewExperience.Should().Be(100 + matchLength);

        Math.Abs(winner.RatingChange + loser.RatingChange).Should().BeLessThan(0.01);
    }
}
