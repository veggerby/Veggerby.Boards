using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Rules.Algorithms
{
    public class Path<T>
    {
        public IEnumerable<Edge<T>> Edges { get; }
        public int Distance => Edges.Sum(x => x.Weight);
        public T From => Edges.First().From;
        public T To => Edges.Last().To;

        public Path(IEnumerable<Edge<T>> edges)
        {
            if (edges == null || !edges.Any())
            {
                throw new ArgumentException("Invalid edges list", nameof(edges));
            }

            Edges = edges.ToList();
        }

        public override string ToString()
        {
            var path = string.Join(" ", Edges.Select(x => $"[{x.Weight}] {x.To}"));
            return $"Path: {From} {path}";
        }
    }
}