using System;

namespace Veggerby.Boards.Core.Artifacts.Patterns;

public class AnyPattern : IPattern, IEquatable<AnyPattern>
{
    public void Accept(IPatternVisitor visitor)
    {
        visitor.Visit(this);
    }

    public bool Equals(AnyPattern other)
    {
        return other is not null;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((AnyPattern)obj);
    }

    public override int GetHashCode() => typeof(AnyPattern).GetHashCode();
}