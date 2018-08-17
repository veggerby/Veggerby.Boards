using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class MultiDirectionPattern : IPattern
    {
        public IEnumerable<Direction> Directions { get; }
        public bool IsRepeatable { get; }

        public MultiDirectionPattern(IEnumerable<Direction> directions, bool isRepeatable = true)
        {
            if (directions == null)
            {
                throw new ArgumentNullException(nameof(directions));
            }

            if (!directions.Any())
            {
                throw new ArgumentException("Empty directions list", nameof(directions));
            }

            Directions = directions.ToList();
            IsRepeatable = isRepeatable;
        }

        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected bool Equals(MultiDirectionPattern other)
        {
            return IsRepeatable == other.IsRepeatable && !Directions.Except(other.Directions).Any() && !other.Directions.Except(Directions).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MultiDirectionPattern)obj);
        }

        public override int GetHashCode()
        {
            return Directions.Aggregate(IsRepeatable.GetHashCode(), (seed, direction) => seed ^ direction.GetHashCode());
        }
    }
}