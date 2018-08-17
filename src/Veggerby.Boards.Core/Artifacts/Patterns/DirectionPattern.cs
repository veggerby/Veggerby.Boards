using System;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class DirectionPattern : IPattern
    {
        public Direction Direction { get; }
        public bool IsRepeatable { get; }

        public DirectionPattern(Direction direction, bool isRepeatable = true)
        {
            if (direction == null)
            {
                throw new ArgumentNullException(nameof(direction));
            }

            Direction = direction;
            IsRepeatable = isRepeatable;
        }

        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected bool Equals(DirectionPattern other)
        {
            return Direction.Equals(other.Direction) && IsRepeatable == other.IsRepeatable;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DirectionPattern)obj);
        }

        public override int GetHashCode()
        {
            return Direction.GetHashCode() ^ IsRepeatable.GetHashCode();
        }
    }
}