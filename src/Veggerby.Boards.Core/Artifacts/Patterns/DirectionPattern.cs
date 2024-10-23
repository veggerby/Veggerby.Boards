using System;

using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns;

public class DirectionPattern : IPattern, IEquatable<DirectionPattern>
{
    public Direction Direction { get; }
    public bool IsRepeatable { get; }

    public DirectionPattern(Direction direction, bool isRepeatable = true)
    {
        ArgumentNullException.ThrowIfNull(direction);

        Direction = direction;
        IsRepeatable = isRepeatable;
    }

    public void Accept(IPatternVisitor visitor)
    {
        visitor.Visit(this);
    }

    public bool Equals(DirectionPattern other)
    {
        return other is not null && Direction.Equals(other.Direction) && IsRepeatable == other.IsRepeatable;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DirectionPattern)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Direction, IsRepeatable);
}