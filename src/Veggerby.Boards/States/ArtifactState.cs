using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Base generic artifact state wrapper providing strongly typed access to the underlying artifact.
/// </summary>
/// <typeparam name="T">Concrete <see cref="Artifact"/> type.</typeparam>
public abstract class ArtifactState<T> : IArtifactState where T : Artifact
{
    /// <summary>
    /// Gets the associated artifact instance.
    /// </summary>
    public T Artifact
    {
        get;
    }

    Artifact IArtifactState.Artifact => Artifact;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactState{T}"/> class.
    /// </summary>
    /// <param name="artifact">Underlying artifact (non-null).</param>
    public ArtifactState(T artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        Artifact = artifact;
    }

    /// <summary>
    /// Determines structural equality with another artifact state.
    /// </summary>
    public abstract bool Equals(IArtifactState other);
}