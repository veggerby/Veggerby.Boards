namespace Veggerby.Boards.Core.Artifacts.Patterns;

public class NullPattern : IPattern
{
    public void Accept(IPatternVisitor visitor)
    {
        visitor.Visit(this);
    }

    public bool Equals(NullPattern other)
    {
        return other is not null;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NullPattern)obj);
    }

    public override int GetHashCode() => typeof(NullPattern).GetHashCode();
}