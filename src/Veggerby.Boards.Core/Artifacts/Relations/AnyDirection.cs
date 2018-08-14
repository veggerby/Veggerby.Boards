namespace Veggerby.Boards.Core.Artifacts.Relations
{
    public class AnyDirection : Direction
    {
        public AnyDirection() : base("any")
        {
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Direction) return true;

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}