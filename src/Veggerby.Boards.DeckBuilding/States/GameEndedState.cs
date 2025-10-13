using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Marker state indicating the deck-building game has terminated.
/// </summary>
internal sealed class GameEndedMarker : Artifact
{
    public GameEndedMarker() : base("game-ended-marker") { }
}

/// <summary>
/// Immutable marker state indicating the deck-building game has reached a terminal condition.
/// </summary>
public sealed class GameEndedState : IArtifactState
{
    private static readonly GameEndedMarker Marker = new();

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other) => other is GameEndedState;
}