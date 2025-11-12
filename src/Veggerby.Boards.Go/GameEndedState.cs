using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go;

/// <summary>
/// Marker artifact indicating the Go game has terminated (two consecutive passes).
/// </summary>
internal sealed class GameEndedMarker : Artifact
{
    public GameEndedMarker() : base("go-game-ended-marker") { }
}

/// <summary>
/// Immutable marker state indicating the Go game has reached a terminal condition (two consecutive passes).
/// </summary>
public sealed class GameEndedState : IArtifactState
{
    private static readonly GameEndedMarker Marker = new();

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other) => other is GameEndedState;
}
