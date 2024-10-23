using System;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

public abstract class ArtifactState<T> : IArtifactState where T : Artifact
{
    public T Artifact { get; }

    Artifact IArtifactState.Artifact => Artifact;

    public ArtifactState(T artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        Artifact = artifact;
    }

    public abstract bool Equals(IArtifactState other);
}