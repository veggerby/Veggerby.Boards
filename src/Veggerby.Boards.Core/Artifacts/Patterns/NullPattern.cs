namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class NullPattern : IPattern
    {
        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected bool Equals(NullPattern other)
        {
            return other != null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NullPattern)obj);
        }

        public override int GetHashCode()
        {
            return typeof(NullPattern).GetHashCode();
        }
    }
}