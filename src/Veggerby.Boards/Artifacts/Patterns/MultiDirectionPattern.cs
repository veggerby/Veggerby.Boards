using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Artifacts.Patterns;

/// <summary>
/// Describes movement that can proceed in any of a supplied set of <see cref="Direction"/> values,
/// optionally repeating in the chosen direction to extend distance.
/// </summary>
public class MultiDirectionPattern : IPattern
{
    /// <summary>
    /// Gets the available directions for the first (and subsequent) step(s).
    /// </summary>
    public IEnumerable<Direction> Directions { get; }

    /// <summary>
    /// Gets a value indicating whether the selected direction may be applied repeatedly.
    /// </summary>
    public bool IsRepeatable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiDirectionPattern"/> class.
    /// </summary>
    /// <param name="directions">Candidate directions for movement. Must contain at least one value.</param>
    /// <param name="isRepeatable">If true a chosen direction can be chained for additional steps.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="directions"/> is empty.</exception>
    public MultiDirectionPattern(IEnumerable<Direction> directions, bool isRepeatable = true)
    {
        ArgumentNullException.ThrowIfNull(directions);

        if (!directions.Any())
        {
            throw new ArgumentException("Empty directions list", nameof(directions));
        }

        Directions = [.. directions];
        IsRepeatable = isRepeatable;
    }

    /// <inheritdoc />
    public void Accept(IPatternVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.Visit(this);
    }

    /// <inheritdoc />
    public bool Equals(MultiDirectionPattern? other)
    {
        return other is not null && IsRepeatable == other.IsRepeatable && !Directions.Except(other.Directions).Any() && !other.Directions.Except(Directions).Any();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MultiDirectionPattern)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(IsRepeatable);
        foreach (var direction in Directions)
        {
            code.Add(direction);
        }

        return code.ToHashCode();
    }
}