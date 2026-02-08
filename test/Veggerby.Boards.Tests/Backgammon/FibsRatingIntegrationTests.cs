using Veggerby.Boards.Backgammon;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Backgammon;

public class FibsRatingIntegrationTests
{
    [Fact]
    public void Should_initialize_fibs_ratings_on_game_progress()
    {
        // arrange
        var progress = new BackgammonGameBuilder().Compile();

        // act
        var actual = progress.WithFibsRatings(matchLength: 5);

        // assert
        var whiteRating = actual.GetFibsRating("white");
        whiteRating.Should().NotBeNull();
        whiteRating!.Rating.Should().Be(1500.0);
        whiteRating.Experience.Should().Be(0);

        var blackRating = actual.GetFibsRating("black");
        blackRating.Should().NotBeNull();
        blackRating!.Rating.Should().Be(1500.0);
        blackRating.Experience.Should().Be(0);

        var matchConfig = actual.GetFibsMatchConfig();
        matchConfig.Should().NotBeNull();
        matchConfig!.MatchLength.Should().Be(5);
        matchConfig.IsUnlimited.Should().BeFalse();
    }

    [Fact]
    public void Should_initialize_fibs_ratings_with_custom_values()
    {
        // arrange
        var progress = new BackgammonGameBuilder().Compile();

        // act
        var actual = progress.WithFibsRatings(
            matchLength: 7,
            whiteRating: 1600.0,
            whiteExperience: 100,
            blackRating: 1400.0,
            blackExperience: 50);

        // assert
        var whiteRating = actual.GetFibsRating("white");
        whiteRating!.Rating.Should().Be(1600.0);
        whiteRating.Experience.Should().Be(100);

        var blackRating = actual.GetFibsRating("black");
        blackRating!.Rating.Should().Be(1400.0);
        blackRating.Experience.Should().Be(50);

        var matchConfig = actual.GetFibsMatchConfig();
        matchConfig!.MatchLength.Should().Be(7);
    }

    [Fact]
    public void Should_initialize_unlimited_match()
    {
        // arrange
        var progress = new BackgammonGameBuilder().Compile();

        // act
        var actual = progress.WithFibsRatings(matchLength: 0, isUnlimited: true);

        // assert
        var matchConfig = actual.GetFibsMatchConfig();
        matchConfig!.IsUnlimited.Should().BeTrue();
    }

    [Fact]
    public void Should_return_null_for_unconfigured_ratings()
    {
        // arrange
        var progress = new BackgammonGameBuilder().Compile();

        // act
        var whiteRating = progress.GetFibsRating("white");
        var blackRating = progress.GetFibsRating("black");
        var matchConfig = progress.GetFibsMatchConfig();

        // assert
        whiteRating.Should().BeNull();
        blackRating.Should().BeNull();
        matchConfig.Should().BeNull();
    }
}
