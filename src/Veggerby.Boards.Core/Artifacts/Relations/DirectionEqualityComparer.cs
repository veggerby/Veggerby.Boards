using System.Collections.Generic;

namespace Veggerby.Boards.Core.Artifacts.Relations;

public class DirectionEqualityComparer : IEqualityComparer<Direction>
{
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

    public int GetHashCode(Direction obj) => obj.GetHashCode();
}