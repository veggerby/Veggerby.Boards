using System.Collections.Generic;

namespace Veggerby.Boards.Core.States;

/// <summary>
/// Compares artifact state instances by underlying artifact identity only (ignoring additional state).
/// </summary>
public class ArtifactStateEqualityComparer : EqualityComparer<IArtifactState>
{
    /// <inheritdoc />
    public override bool Equals(IArtifactState x, IArtifactState y)
    {
        return x?.Artifact.Equals(y?.Artifact) ?? false;
    }

    /// <inheritdoc />
    public override int GetHashCode(IArtifactState obj) => obj.Artifact.GetHashCode();
}