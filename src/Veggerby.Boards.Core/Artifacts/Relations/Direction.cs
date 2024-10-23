using System;

namespace Veggerby.Boards.Core.Artifacts.Relations;

public class Direction : IEquatable<Direction>
{
    public static readonly Direction Left = new("left");
    public static readonly Direction Right = new("right");
    public static readonly Direction Up = new("up");
    public static readonly Direction Down = new("down");
    public static readonly Direction Across = new("across");

    public static readonly Direction North = new("north");
    public static readonly Direction South = new("south");
    public static readonly Direction East = new("east");
    public static readonly Direction West = new("west");

    public static readonly Direction NorthWest = new("north-west");
    public static readonly Direction NorthEast = new("north-east");
    public static readonly Direction SouthWest = new("south-west");
    public static readonly Direction SouthEast = new("south-east");

    public static readonly Direction Clockwise = new("clockwise");
    public static readonly Direction CounterClockwise = new("counter-clockwise");

    public static readonly Direction Any = new AnyDirection();

    public string Id { get; }

    public Direction(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Invalid Direction Id", nameof(id));
        }

        Id = id;
    }

    public bool Equals(Direction other)
    {
        return other is not null && string.Equals(Id, other.Id);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is AnyDirection) return true;
        if (obj.GetType() != this.GetType()) return false;

        return Equals((Direction)obj);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => $"{GetType().Name} {Id}";
}
