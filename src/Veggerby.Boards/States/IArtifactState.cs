using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Represents the immutable state associated with a single <see cref="Artifact"/>.
/// </summary>
/// <remarks>
/// All concrete implementations must be value based and suitable for structural comparison. Implementations
/// participate in <see cref="GameState"/> diffs via <see cref="ArtifactStateChange"/>.
/// </remarks>
public interface IArtifactState
{
    /// <summary>
    /// Gets the artifact this state instance belongs to.
    /// </summary>
    Artifact Artifact
    {
        get;
    }

    /// <summary>
    /// Gets the visibility constraint for this artifact state in player-projected views.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Visibility.Public"/> for backward compatibility with existing implementations.
    /// Override to return <see cref="Visibility.Private"/> or <see cref="Visibility.Hidden"/> for
    /// imperfect-information games. Visibility is typically computed based on the state's properties
    /// (e.g., a card's face-up/face-down status).
    /// </remarks>
    Visibility Visibility => Visibility.Public;
}