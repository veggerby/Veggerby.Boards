namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Global immutable artifact representing the staging area for simultaneous player commitments.
/// </summary>
/// <remarks>
/// A single <see cref="StagedEventsArtifact"/> is created when a game uses simultaneous commitment phases.
/// Its associated <c>StagedEventsState</c> tracks pending and committed player actions that will be
/// revealed and resolved together once all required players have committed.
/// </remarks>
/// <param name="id">Artifact identifier (stable, typically a fixed constant).</param>
public sealed class StagedEventsArtifact(string id) : Artifact(id)
{
}
