using System;

namespace Veggerby.Boards.Core.Phases
{
    public class Turn
    {
        public Round Round { get; }
        public int Number { get; }

        public Turn(Round round, int number)
        {
            if (round == null)
            {
                throw new ArgumentNullException(nameof(round));
            }

            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Turn number must be positive and non-zero");
            }
            
            Round = round;
            Number = number;
        }

        protected bool Equals(Turn other)
        {
            return Number.Equals(other.Number) && Round.Equals(other.Round);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Turn)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Number.GetHashCode();
                hashCode = (hashCode*397) ^ (Round?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}