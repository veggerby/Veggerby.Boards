using System;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Represents a movement direction between tiles. Predefined static instances cover common compass and board directions.
/// </summary>
public class Direction : IEquatable<Direction>
{
    /// <summary>Left.</summary>
    public static readonly Direction Left = new("left");
    /// <summary>Right.</summary>
    public static readonly Direction Right = new("right");
    /// <summary>Up.</summary>
    public static readonly Direction Up = new("up");
    /// <summary>Down.</summary>
    public static readonly Direction Down = new("down");
    /// <summary>Across / lateral.</summary>
    public static readonly Direction Across = new("across");

    /// <summary>North.</summary>
    public static readonly Direction North = new("north");
    /// <summary>South.</summary>
    public static readonly Direction South = new("south");
    /// <summary>East.</summary>
    public static readonly Direction East = new("east");
    /// <summary>West.</summary>
    public static readonly Direction West = new("west");

    /// <summary>North-West diagonal.</summary>
    public static readonly Direction NorthWest = new("north-west");
    /// <summary>North-East diagonal.</summary>
    public static readonly Direction NorthEast = new("north-east");
    /// <summary>South-West diagonal.</summary>
    public static readonly Direction SouthWest = new("south-west");
    /// <summary>South-East diagonal.</summary>
    public static readonly Direction SouthEast = new("south-east");

    /// <summary>Clockwise rotation direction.</summary>
    public static readonly Direction Clockwise = new("clockwise");
    /// <summary>Counter-clockwise rotation direction.</summary>
    public static readonly Direction CounterClockwise = new("counter-clockwise");

    /// <summary>Wildcard direction matching any.</summary>
    public static readonly Direction Any = new AnyDirection();

    /// <summary>
    /// Gets the direction identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Initializes a new direction with an identifier.
    /// </summary>
    /// <param name="id">Direction identifier.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is null or empty.</exception>
    public Direction(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Invalid Direction Id", nameof(id));
        }

        Id = id;
    }

    /// <inheritdoc />
    public bool Equals(Direction other)
    {
        return other is not null && string.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is AnyDirection) return true;
        if (obj.GetType() != this.GetType()) return false;

        return Equals((Direction)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => $"{GetType().Name} {Id}";
}