using System.Collections.Generic;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Equality comparer for <see cref="Direction"/> that treats the wildcard <see cref="Direction.Any"/> as equal to any direction.
/// </summary>
public class DirectionEqualityComparer : IEqualityComparer<Direction>
{
    /// <inheritdoc />
    public bool Equals(Direction x, Direction y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        if (x is AnyDirection || y is AnyDirection)
        {
            return true;
        }

        return x.Equals(y);
    }

    /// <inheritdoc />
    public int GetHashCode(Direction obj) => obj.GetHashCode();
}