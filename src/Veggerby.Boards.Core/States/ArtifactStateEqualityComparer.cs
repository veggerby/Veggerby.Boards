using System.Collections.Generic;

namespace Veggerby.Boards.Core.States;

public class ArtifactStateEqualityComparer : EqualityComparer<IArtifactState>
{
    public override bool Equals(IArtifactState x, IArtifactState y)
    {
        return x?.Artifact.Equals(y?.Artifact) ?? false;
    }

    public override int GetHashCode(IArtifactState obj) => obj.Artifact.GetHashCode();
}