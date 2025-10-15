using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable snapshot of the global turn timeline capturing the current turn number and segment.
/// </summary>
/// <remarks>
/// Introduced in shadow mode: no mutators currently advance or transition segments. Future work will
/// add deterministic turn progression and rule gating keyed off <see cref="TurnSegment"/>.
/// Equality includes both numeric turn and segment to ensure state diffing captures transitions.
/// </remarks>
public sealed class TurnState : ArtifactState<TurnArtifact>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TurnState"/> class.
    /// </summary>
    /// <param name="artifact">Associated <see cref="TurnArtifact"/>.</param>
    /// <param name="turnNumber">Current (1-based) turn number.</param>
    /// <param name="segment">Current segment within the turn.</param>
    /// <param name="passStreak">Current consecutive pass streak (0 for none).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="turnNumber" /> &lt; 1.</exception>
    public TurnState(TurnArtifact artifact, int turnNumber, TurnSegment segment, int passStreak = 0) : base(artifact)
    {
        if (turnNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), "Turn number must be >= 1");
        }
        if (passStreak < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(passStreak), "Pass streak must be >= 0");
        }

        TurnNumber = turnNumber;
        Segment = segment;
        PassStreak = passStreak;
    }

    /// <summary>
    /// Gets the current 1-based turn number.
    /// </summary>
    public int TurnNumber { get; }

    /// <summary>
    /// Gets the current segment inside the turn.
    /// </summary>
    public TurnSegment Segment { get; }

    /// <summary>
    /// Gets the current consecutive pass streak count (reset when a non-pass advancement occurs, incremented by pass event). Used by games like Go for two-pass termination.
    /// </summary>
    public int PassStreak { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as TurnState);
    }

    /// <inheritdoc />
    public override bool Equals(IArtifactState other)
    {
        return Equals(other as TurnState);
    }

    /// <summary>
    /// Determines equality with another <see cref="TurnState"/> considering turn number and segment.
    /// </summary>
    /// <param name="other">Other state.</param>
    /// <returns><c>true</c> if both reference the same artifact and have identical number + segment.</returns>
    public bool Equals(TurnState? other)
    {
        if (other is null)
        {
            return false;
        }

        return Artifact.Equals(other.Artifact)
            && TurnNumber.Equals(other.TurnNumber)
            && Segment.Equals(other.Segment)
            && PassStreak.Equals(other.PassStreak);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Artifact, TurnNumber, Segment, PassStreak);
}