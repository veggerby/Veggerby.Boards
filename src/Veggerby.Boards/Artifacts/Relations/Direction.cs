using System;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Represents a movement direction between tiles. Predefined static instances cover common compass and board directions.
/// </summary>
public class Direction : IEquatable<Direction>
{
    /// <summary>Left.</summary>
    public static readonly Direction Left = new(Constants.Directions.Left);
    /// <summary>Right.</summary>
    public static readonly Direction Right = new(Constants.Directions.Right);
    /// <summary>Up.</summary>
    public static readonly Direction Up = new(Constants.Directions.Up);
    /// <summary>Down.</summary>
    public static readonly Direction Down = new(Constants.Directions.Down);
    /// <summary>Across / lateral.</summary>
    public static readonly Direction Across = new(Constants.Directions.Across);

    /// <summary>North.</summary>
    public static readonly Direction North = new(Constants.Directions.North);
    /// <summary>South.</summary>
    public static readonly Direction South = new(Constants.Directions.South);
    /// <summary>East.</summary>
    public static readonly Direction East = new(Constants.Directions.East);
    /// <summary>West.</summary>
    public static readonly Direction West = new(Constants.Directions.West);

    /// <summary>North-West diagonal.</summary>
    public static readonly Direction NorthWest = new(Constants.Directions.NorthWest);
    /// <summary>North-East diagonal.</summary>
    public static readonly Direction NorthEast = new(Constants.Directions.NorthEast);
    /// <summary>South-West diagonal.</summary>
    public static readonly Direction SouthWest = new(Constants.Directions.SouthWest);
    /// <summary>South-East diagonal.</summary>
    public static readonly Direction SouthEast = new(Constants.Directions.SouthEast);

    /// <summary>Clockwise rotation direction.</summary>
    public static readonly Direction Clockwise = new(Constants.Directions.Clockwise);
    /// <summary>Counter-clockwise rotation direction.</summary>
    public static readonly Direction CounterClockwise = new(Constants.Directions.CounterClockwise);

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