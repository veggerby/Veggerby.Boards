using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Extension methods for configuring FIBS rating system in Backgammon games.
/// </summary>
public static class BackgammonFibsExtensions
{
    /// <summary>
    /// Initializes FIBS rating tracking for a Backgammon game.
    /// </summary>
    /// <param name="progress">The current game progress.</param>
    /// <param name="matchLength">The agreed-upon match length (points to win). Common values: 1, 3, 5, 7, 9, 11, 13.</param>
    /// <param name="isUnlimited">Whether this is an unlimited match (ratings not updated).</param>
    /// <param name="whiteRating">Initial rating for white player (default: 1500).</param>
    /// <param name="whiteExperience">Initial experience for white player (default: 0).</param>
    /// <param name="blackRating">Initial rating for black player (default: 1500).</param>
    /// <param name="blackExperience">Initial experience for black player (default: 0).</param>
    /// <returns>Updated game progress with FIBS rating states initialized.</returns>
    /// <remarks>
    /// FIBS (First Internet Backgammon Server) rating system tracks player skill levels.
    /// Ratings start at 1500 for new players and adjust based on match results, match length,
    /// and player experience. Experience determines rating volatility (players with less than
    /// 400 experience have more volatile ratings).
    /// 
    /// This should be called on the initial game progress before any moves are made.
    /// </remarks>
    public static GameProgress WithFibsRatings(
        this GameProgress progress,
        int matchLength,
        bool isUnlimited = false,
        double whiteRating = 1500.0,
        int whiteExperience = 0,
        double blackRating = 1500.0,
        int blackExperience = 0)
    {
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        var white = progress.Game.GetPlayer("white");
        var black = progress.Game.GetPlayer("black");

        if (white == null || black == null)
        {
            throw new InvalidOperationException("Backgammon game must have 'white' and 'black' players.");
        }

        var matchConfig = new FibsMatchConfigState(matchLength, isUnlimited);
        var whiteRatingState = new FibsRatingState(white, whiteRating, whiteExperience);
        var blackRatingState = new FibsRatingState(black, blackRating, blackExperience);

        return progress.NewState(new IArtifactState[]
        {
            matchConfig,
            whiteRatingState,
            blackRatingState
        });
    }

    /// <summary>
    /// Gets the current FIBS rating for a player.
    /// </summary>
    /// <param name="progress">The current game progress.</param>
    /// <param name="playerId">The player ID ("white" or "black").</param>
    /// <returns>The current rating state, or null if FIBS ratings are not configured.</returns>
    public static FibsRatingState? GetFibsRating(this GameProgress progress, string playerId)
    {
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        var player = progress.Game.GetPlayer(playerId);

        if (player == null)
        {
            return null;
        }

        return progress.State.GetStates<FibsRatingState>()
            .FirstOrDefault(r => Equals(r.Player, player));
    }

    /// <summary>
    /// Gets the match configuration for FIBS ratings.
    /// </summary>
    /// <param name="progress">The current game progress.</param>
    /// <returns>The match configuration, or null if FIBS ratings are not configured.</returns>
    public static FibsMatchConfigState? GetFibsMatchConfig(this GameProgress progress)
    {
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        return progress.State.GetStates<FibsMatchConfigState>().FirstOrDefault();
    }
}