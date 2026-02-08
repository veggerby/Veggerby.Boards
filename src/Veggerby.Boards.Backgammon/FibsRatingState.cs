using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Marker artifact for FIBS rating state.
/// </summary>
internal sealed class FibsRatingMarker : Artifact
{
    public FibsRatingMarker(Player player) : base($"fibs-rating-{player.Id}") { }
}

/// <summary>
/// Immutable state representing a player's FIBS rating and experience.
/// </summary>
/// <remarks>
/// FIBS (First Internet Backgammon Server) rating system tracks player skill levels
/// with initial rating of 1500 and experience (cumulative match length) starting at 0.
/// Players with experience less than 400 have more volatile ratings.
/// </remarks>
public sealed class FibsRatingState : IArtifactState
{
    private readonly FibsRatingMarker _marker;

    /// <summary>
    /// Gets the player this rating belongs to.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the current FIBS rating.
    /// </summary>
    /// <remarks>
    /// New players start at 1500. Typical range is 1000-2000.
    /// </remarks>
    public double Rating
    {
        get;
    }

    /// <summary>
    /// Gets the cumulative experience (sum of all match lengths played).
    /// </summary>
    /// <remarks>
    /// Experience determines rating volatility. Players with experience less than 400
    /// have a volatility factor that decreases as they gain experience.
    /// </remarks>
    public int Experience
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FibsRatingState"/> class.
    /// </summary>
    /// <param name="player">The player this rating belongs to.</param>
    /// <param name="rating">The current FIBS rating (default: 1500 for new players).</param>
    /// <param name="experience">The cumulative experience (default: 0).</param>
    public FibsRatingState(Player player, double rating = 1500.0, int experience = 0)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Rating = rating;
        Experience = experience;
        _marker = new FibsRatingMarker(player);
    }

    /// <inheritdoc />
    public Artifact Artifact => _marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState? other)
    {
        return other is FibsRatingState frs &&
               Equals(frs.Player, Player) &&
               Math.Abs(frs.Rating - Rating) < 0.0001 &&
               frs.Experience == Experience;
    }
}
