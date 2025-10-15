using System;

namespace Veggerby.Boards.States;

/// <summary>
/// Represents the delta for a single artifact between two <see cref="GameState"/> snapshots.
/// </summary>
public class ArtifactStateChange
{
    /// <summary>
    /// Gets the previous state (null if newly created in the target state).
    /// </summary>
    public IArtifactState? From { get; }

    /// <summary>
    /// Gets the new state (null if removed in the target state).
    /// </summary>
    public IArtifactState? To { get; }

    /// <summary>
    /// Initializes a new instance ensuring both states (when present) reference the same artifact.
    /// </summary>
    /// <param name="from">Previous artifact state (may be null).</param>
    /// <param name="to">New artifact state (may be null).</param>
    /// <exception cref="ArgumentNullException">Thrown if both <paramref name="from"/> and <paramref name="to"/> are null.</exception>
    /// <exception cref="ArgumentException">Thrown if both present but reference different artifacts.</exception>
    public ArtifactStateChange(IArtifactState? from, IArtifactState? to)
    {
        if (from is null && to is null)
        {
            throw new ArgumentNullException(nameof(to), "Both From and To cannot be null");
        }

        if (from is not null && to is not null && !from.Artifact.Equals(to.Artifact))
        {
            throw new ArgumentException("To and From need to reference the same artifact", nameof(to));
        }

        From = from;
        To = to;
    }
}