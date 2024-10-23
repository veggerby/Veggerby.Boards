using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns;

public class FixedPattern : IPattern
{
    public IEnumerable<Direction> Pattern { get; }

    public FixedPattern(IEnumerable<Direction> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (!pattern.Any())
        {
            throw new ArgumentException("Empty pattern list", nameof(pattern));
        }

        Pattern = pattern.ToList();
    }

    public void Accept(IPatternVisitor visitor)
    {
        visitor.Visit(this);
    }

    public bool Equals(FixedPattern other)
    {
        return other is not null && Pattern.SequenceEqual(other.Pattern);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((FixedPattern)obj);
    }

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