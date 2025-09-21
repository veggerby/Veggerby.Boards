using System;

using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns;

/// <summary>
/// Describes movement along a single <see cref="Direction"/> optionally repeated multiple steps.
/// </summary>
public class DirectionPattern : IPattern, IEquatable<DirectionPattern>
{
    /// <summary>
    /// Gets the direction of travel.
    /// </summary>
    public Direction Direction { get; }

    /// <summary>
    /// Gets a value indicating whether the direction may be repeated to extend the path.
    /// </summary>
    public bool IsRepeatable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionPattern"/> class.
    /// </summary>
    /// <param name="direction">The direction of travel.</param>
    /// <param name="isRepeatable">If true the direction can be applied successively producing longer paths.</param>
    public DirectionPattern(Direction direction, bool isRepeatable = true)
    {
        ArgumentNullException.ThrowIfNull(direction);

        Direction = direction;
        IsRepeatable = isRepeatable;
    }

    /// <inheritdoc />
    public void Accept(IPatternVisitor visitor)
    {
        visitor.Visit(this);
    }

    /// <inheritdoc />
    public bool Equals(DirectionPattern other)
    {
        return other is not null && Direction.Equals(other.Direction) && IsRepeatable == other.IsRepeatable;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DirectionPattern)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Direction, IsRepeatable);
}