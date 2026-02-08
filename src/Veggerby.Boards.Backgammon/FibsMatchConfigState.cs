using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Marker artifact for FIBS match configuration.
/// </summary>
internal sealed class FibsMatchConfigMarker : Artifact
{
    public FibsMatchConfigMarker() : base("fibs-match-config") { }
}

/// <summary>
/// Immutable state representing FIBS match configuration.
/// </summary>
/// <remarks>
/// Match configuration determines the agreed-upon match length and whether
/// the match is unlimited (which excludes it from rating calculations).
/// </remarks>
public sealed class FibsMatchConfigState : IArtifactState
{
    private static readonly FibsMatchConfigMarker Marker = new();

    /// <summary>
    /// Gets the agreed-upon match length (number of points to win).
    /// </summary>
    /// <remarks>
    /// This is not the final score but the target score agreed upon before the match.
    /// Common values: 1, 3, 5, 7, 9, 11, 13.
    /// </remarks>
    public int MatchLength
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether this is an unlimited match.
    /// </summary>
    /// <remarks>
    /// Unlimited matches do not affect FIBS ratings.
    /// </remarks>
    public bool IsUnlimited
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FibsMatchConfigState"/> class.
    /// </summary>
    /// <param name="matchLength">The agreed-upon match length.</param>
    /// <param name="isUnlimited">Whether this is an unlimited match.</param>
    public FibsMatchConfigState(int matchLength, bool isUnlimited = false)
    {
        if (matchLength <= 0 && !isUnlimited)
        {
            throw new ArgumentException("Match length must be positive for rated matches", nameof(matchLength));
        }

        MatchLength = matchLength;
        IsUnlimited = isUnlimited;
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState? other)
    {
        return other is FibsMatchConfigState fmcs &&
               fmcs.MatchLength == MatchLength &&
               fmcs.IsUnlimited == IsUnlimited;
    }
}
