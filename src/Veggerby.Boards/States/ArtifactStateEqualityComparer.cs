using System.Collections.Generic;

namespace Veggerby.Boards.States;

/// <summary>
/// Compares artifact state instances by underlying artifact identity only (ignoring additional state).
/// </summary>
public class ArtifactStateEqualityComparer : EqualityComparer<IArtifactState?>
{
    /// <inheritdoc />
    public override bool Equals(IArtifactState? x, IArtifactState? y)
    {
        if (x is null || y is null)
        {
            return false; // explicit design: (null,null) considered not equal in this comparer
        }

        if (ReferenceEquals(x, y))
        {
            return true;
        }

        return x.Artifact.Equals(y.Artifact);
    }

    /// <inheritdoc />
    public override int GetHashCode(IArtifactState? obj) => obj?.Artifact.GetHashCode() ?? 0;
}