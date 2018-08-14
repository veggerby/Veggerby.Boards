using System;

namespace Veggerby.Boards.Core.Flows.Rules.Algorithms
{
    public class Edge<T>
    {
        public T From { get; }
        public T To { get; }
        public int Weight { get; }

        public Edge(T from, T to, int weight)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            From = from;
            To = to;
            Weight = weight;
        }

        protected bool Equals(Edge<T> other)
        {
            return Equals(From, other.From) && Equals(To, other.To) && Equals(Weight, other.Weight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Edge<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = From?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (To?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Weight.GetHashCode();
                return hashCode;
            }
        }
    }
}