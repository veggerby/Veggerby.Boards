using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Marker artifact indicating the game has terminated.
/// </summary>
internal sealed class GameEndedMarker : Artifact
{
    public GameEndedMarker() : base("game-ended-marker") { }
}

/// <summary>
/// Immutable marker state indicating the game has reached a terminal condition.
/// </summary>
/// <remarks>
/// This state serves as a universal signal that the game has concluded. Game modules
/// may supplement this marker with module-specific outcome states (e.g., ChessOutcomeState,
/// GoOutcomeState) to provide additional details about the termination reason and final results.
/// </remarks>
public sealed class GameEndedState : IArtifactState
{
    private static readonly GameEndedMarker Marker = new();

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other) => other is GameEndedState;
}
