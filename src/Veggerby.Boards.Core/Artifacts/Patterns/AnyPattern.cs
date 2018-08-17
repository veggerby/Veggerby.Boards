namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class AnyPattern : IPattern
    {
        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected bool Equals(AnyPattern other)
        {
            return other != null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnyPattern)obj);
        }

        public override int GetHashCode()
        {
            return typeof(AnyPattern).GetHashCode();
        }
    }
}