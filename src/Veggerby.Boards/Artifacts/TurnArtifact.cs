using System;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Global immutable artifact representing the logical turn timeline for a game instance.
/// </summary>
/// <remarks>
/// A single <see cref="TurnArtifact"/> is created per compiled game (shadow mode initially). Its
/// associated <c>TurnState</c> (introduced separately) will track the current turn number and segment
/// once sequencing logic is activated. Presence of this artifact enables future rules and mutators to
/// evolve turns without overloading <see cref="Player"/> state.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="TurnArtifact"/> class.
/// </remarks>
/// <param name="id">Artifact identifier (stable, typically a fixed constant).</param>
public sealed class TurnArtifact(string id) : Artifact(id)
{
}