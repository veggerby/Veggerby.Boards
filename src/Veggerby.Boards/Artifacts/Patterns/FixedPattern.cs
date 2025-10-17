using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Artifacts.Patterns;

/// <summary>
/// Describes a rigid sequence of <see cref="Direction"/> steps that must all be available for the move to be valid.
/// </summary>
public class FixedPattern : IPattern
{
    /// <summary>
    /// Gets the ordered sequence of directions composing the pattern.
    /// </summary>
    public IEnumerable<Direction> Pattern { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedPattern"/> class.
    /// </summary>
    /// <param name="pattern">The ordered directions that must be followed. Must contain at least one element.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> is empty.</exception>
    public FixedPattern(IEnumerable<Direction> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (!pattern.Any())
        {
            throw new ArgumentException("Empty pattern list", nameof(pattern));
        }

        Pattern = [.. pattern];
    }

    /// <inheritdoc />
    public void Accept(IPatternVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.Visit(this);
    }

    /// <inheritdoc />
    public bool Equals(FixedPattern? other)
    {
        return other is not null && Pattern.SequenceEqual(other.Pattern);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((FixedPattern)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        foreach (var direction in Pattern)
        {
            code.Add(direction);
        }
        return code.ToHashCode();
    }
}