using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Flows.Rules.Algorithms
{
    public class Graph<T>
    {
        public IEnumerable<T> Vertices { get; }
        public IEnumerable<Edge<T>> Edges { get; }

        public Graph(IEnumerable<T> vertices, IEnumerable<Edge<T>> edges)
        {
            Vertices = (vertices ?? Enumerable.Empty<T>()).ToList();
            Edges = (edges ?? Enumerable.Empty<Edge<T>>()).ToList();
        }

        public Edge<T> GetEdge(T from, T to)
        {
            return Edges.SingleOrDefault(x => x.From.Equals(from) && x.To.Equals(to));
        }
    }
}