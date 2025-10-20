using System;

namespace Veggerby.Boards.Artifacts.Patterns;

/// <summary>
/// Represents a wildcard pattern that by itself imposes no movement constraint.
/// actual interpretation is delegated to the visitor implementation.
/// </summary>
public class AnyPattern : IPattern, IEquatable<AnyPattern>
{
    /// <inheritdoc />
    public void Accept(IPatternVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        visitor.Visit(this);
    }

    /// <inheritdoc />
    public bool Equals(AnyPattern? other)
    {
        return other is not null;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((AnyPattern)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => typeof(AnyPattern).GetHashCode();
}