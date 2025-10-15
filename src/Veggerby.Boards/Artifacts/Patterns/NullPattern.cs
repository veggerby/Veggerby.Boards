namespace Veggerby.Boards.Artifacts.Patterns;

/// <summary>
/// Represents an empty / no-op pattern (used as a placeholder or explicit absence of movement).
/// </summary>
public class NullPattern : IPattern
{
    /// <inheritdoc />
    public void Accept(IPatternVisitor visitor)
    {
        visitor.Visit(this);
    }

    /// <summary>
    /// Compares two <see cref="NullPattern"/> instances (all instances are considered equal).
    /// </summary>
    public static bool Equals(NullPattern? other)
    {
        return other is not null;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NullPattern)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => typeof(NullPattern).GetHashCode();
}