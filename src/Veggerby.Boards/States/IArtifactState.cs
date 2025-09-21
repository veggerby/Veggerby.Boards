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
    Artifact Artifact { get; }
}