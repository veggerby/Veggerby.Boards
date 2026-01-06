using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Defines visibility rules for filtering artifact states in player-projected views.
/// </summary>
/// <remarks>
/// Visibility policies determine which states a player or observer can see, and how hidden
/// states should be redacted. Implementations must be pure functions (no side effects) and
/// deterministic (same inputs produce same outputs).
/// </remarks>
public interface IVisibilityPolicy
{
    /// <summary>
    /// Determines whether the specified viewer can see the given artifact state.
    /// </summary>
    /// <param name="viewer">The player or observer requesting visibility (may be null for observer contexts).</param>
    /// <param name="state">The artifact state to evaluate.</param>
    /// <returns><c>true</c> if the viewer can see this state; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// This method must be deterministic and thread-safe. The viewer may be null when evaluating
    /// observer roles that do not have an associated player identity.
    /// </remarks>
    bool CanSee(Player? viewer, IArtifactState state);

    /// <summary>
    /// Creates a redacted version of the artifact state for viewers who cannot see the full state.
    /// </summary>
    /// <param name="viewer">The player or observer requesting visibility (may be null for observer contexts).</param>
    /// <param name="state">The artifact state to potentially redact.</param>
    /// <returns>
    /// The original state if visible to the viewer, a redacted placeholder if hidden, or <c>null</c>
    /// if the state should be completely omitted from the projection.
    /// </returns>
    /// <remarks>
    /// This method is called after <see cref="CanSee"/> returns <c>false</c>. Implementations may
    /// return null to exclude the state entirely, or return a placeholder state (e.g., showing that
    /// a card exists but not its value). Must be deterministic and thread-safe.
    /// </remarks>
    IArtifactState? Redact(Player? viewer, IArtifactState state);
}
