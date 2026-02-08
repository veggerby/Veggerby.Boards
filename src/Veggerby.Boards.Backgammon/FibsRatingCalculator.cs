using System;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Pure functions for calculating FIBS (First Internet Backgammon Server) rating changes.
/// </summary>
/// <remarks>
/// The FIBS rating system calculates rating changes based on:
/// 1. Match length (agreed upon, not final score)
/// 2. Player experience (cumulative match length)
/// 3. Rating differential (probability of winning)
///
/// Formula:
/// - D = |P1 - P2| (rating difference)
/// - U = 1/(10^(D*√n/2000)+1) (underdog win probability)
/// - F = 1 - U (favorite win probability)
/// - PE = max(1, 5-((E+n)/100)) (experience factor)
/// - Rating change = ±4 × PE × √n × (U or F depending on player role)
///
/// Reference: https://bkgm.com/articles/McCool/ratings.html
/// </remarks>
public static class FibsRatingCalculator
{
    private const double BaseRatingChange = 4.0;
    private const double ExperienceScale = 100.0;
    private const double ExperienceBase = 5.0;
    private const double MinExperienceFactor = 1.0;
    private const double RatingDifferentialScale = 2000.0;

    /// <summary>
    /// Represents the result of a FIBS rating calculation for a single player.
    /// </summary>
    public sealed class RatingChangeResult
    {
        /// <summary>
        /// Gets the rating change (can be positive or negative).
        /// </summary>
        public double RatingChange
        {
            get;
        }

        /// <summary>
        /// Gets the new rating after applying the change.
        /// </summary>
        public double NewRating
        {
            get;
        }

        /// <summary>
        /// Gets the new experience after adding the match length.
        /// </summary>
        public int NewExperience
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingChangeResult"/> class.
        /// </summary>
        public RatingChangeResult(double ratingChange, double newRating, int newExperience)
        {
            RatingChange = ratingChange;
            NewRating = newRating;
            NewExperience = newExperience;
        }
    }

    /// <summary>
    /// Calculates rating changes for both players after a match completion.
    /// </summary>
    /// <param name="winnerRating">The winner's current rating.</param>
    /// <param name="winnerExperience">The winner's current experience.</param>
    /// <param name="loserRating">The loser's current rating.</param>
    /// <param name="loserExperience">The loser's current experience.</param>
    /// <param name="matchLength">The agreed-upon match length.</param>
    /// <returns>A tuple containing the rating change results for (winner, loser).</returns>
    public static (RatingChangeResult Winner, RatingChangeResult Loser) CalculateRatingChanges(
        double winnerRating,
        int winnerExperience,
        double loserRating,
        int loserExperience,
        int matchLength)
    {
        if (matchLength <= 0)
        {
            throw new ArgumentException("Match length must be positive", nameof(matchLength));
        }

        var ratingDifference = Math.Abs(winnerRating - loserRating);
        var sqrtMatchLength = Math.Sqrt(matchLength);

        var underdogWinProbability = CalculateUnderdogWinProbability(ratingDifference, sqrtMatchLength);
        var favoriteWinProbability = 1.0 - underdogWinProbability;

        var winnerIsHigherRated = winnerRating >= loserRating;

        var winnerExperienceFactor = CalculateExperienceFactor(winnerExperience, matchLength);
        var loserExperienceFactor = CalculateExperienceFactor(loserExperience, matchLength);

        var winnerProbability = winnerIsHigherRated ? underdogWinProbability : favoriteWinProbability;
        var loserProbability = winnerIsHigherRated ? favoriteWinProbability : underdogWinProbability;

        var winnerChange = BaseRatingChange * winnerExperienceFactor * sqrtMatchLength * winnerProbability;
        var loserChange = -BaseRatingChange * loserExperienceFactor * sqrtMatchLength * loserProbability;

        var winnerNewRating = winnerRating + winnerChange;
        var loserNewRating = loserRating + loserChange;

        var winnerNewExperience = winnerExperience + matchLength;
        var loserNewExperience = loserExperience + matchLength;

        return (
            new RatingChangeResult(winnerChange, winnerNewRating, winnerNewExperience),
            new RatingChangeResult(loserChange, loserNewRating, loserNewExperience)
        );
    }

    /// <summary>
    /// Calculates the underdog's win probability based on rating difference and match length.
    /// </summary>
    /// <param name="ratingDifference">The absolute rating difference between players.</param>
    /// <param name="sqrtMatchLength">The square root of the match length.</param>
    /// <returns>The probability (0.0 to 1.0) that the underdog wins.</returns>
    /// <remarks>
    /// Formula: U = 1/(10^(D*√n/2000)+1)
    /// where D is rating difference and √n is square root of match length.
    /// </remarks>
    public static double CalculateUnderdogWinProbability(double ratingDifference, double sqrtMatchLength)
    {
        var exponent = (ratingDifference * sqrtMatchLength) / RatingDifferentialScale;
        return 1.0 / (Math.Pow(10, exponent) + 1.0);
    }

    /// <summary>
    /// Calculates the experience factor (volatility) for a player.
    /// </summary>
    /// <param name="experience">The player's current experience.</param>
    /// <param name="matchLength">The match length being added.</param>
    /// <returns>The experience factor (minimum 1.0).</returns>
    /// <remarks>
    /// Formula: PE = max(1, 5-((E+n)/100))
    /// Players with experience less than 400 have volatility greater than 1.
    /// </remarks>
    public static double CalculateExperienceFactor(int experience, int matchLength)
    {
        var factor = ExperienceBase - ((experience + matchLength) / ExperienceScale);
        return Math.Max(MinExperienceFactor, factor);
    }
}
