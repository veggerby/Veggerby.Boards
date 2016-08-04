using System;

namespace Veggerby.Boards.Core.Phases
{
    public class Round
    {
        public int Number { get; }

        public Round(int number)
        {
            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Round number must be positive and non-zero");
            }
            
            Number = number;
        }

        protected bool Equals(Round other)
        {
            return Number == other.Number;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Round)obj);
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }
    }
}