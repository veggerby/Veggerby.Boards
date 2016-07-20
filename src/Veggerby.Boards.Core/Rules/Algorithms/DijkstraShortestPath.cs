using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Rules.Algorithms
{
    public class DijkstraShortestPath
    {
        public IEnumerable<Path<T>> GetShortestPath<T>(T from, Graph<T> graph)
        {
            var settled = new List<T>();
            var unsettled = new List<T>(graph.Vertices);
            var distances = graph.Vertices.ToDictionary(x => x, x => from.Equals(x) ? 0 : int.MaxValue);
            var parent = new Dictionary<T, T>();

            while (unsettled.Any())
            {
                var evaluationnode = GetNodeWithMinimumDistanceFromUnsettled(unsettled, distances);
                unsettled.Remove(evaluationnode);
                settled.Add(evaluationnode);
                EvaluateNeighbours(evaluationnode, settled, graph, unsettled, distances, parent);
            }

            var result = new List<Path<T>>();
            foreach (var to in graph.Vertices)
            {
                if (!from.Equals(to))
                {
                    result.Add(GetShortestPath(from, to, parent, graph));
                }
            }

            return result;
        }

        private Path<T> GetShortestPath<T>(T from, T to, IDictionary<T, T> parent, Graph<T> graph)
        {
            var shortestPath = new List<Edge<T>>();
            var current = to;

            while(!from.Equals(current)) 
            {
                shortestPath.Add(graph.GetEdge(parent[current], current));
                current = parent[current];
            }

            shortestPath.Reverse();
            return new Path<T>(shortestPath);
        }

        private T GetNodeWithMinimumDistanceFromUnsettled<T>(IEnumerable<T> unsettled, IDictionary<T, int> distances)
        {
            return unsettled
                .OrderBy(x => distances.ContainsKey(x) ? distances[x] : int.MaxValue)
                .FirstOrDefault();
        }
    
        private void EvaluateNeighbours<T>(T evaluationNode, IEnumerable<T> settled, Graph<T> graph, IList<T> unsettled, IDictionary<T, int> distances, IDictionary<T, T> parent)
        {
            foreach (var destinationNode in graph.Vertices)
            {
                if (!settled.Contains(destinationNode))
                {
                    var edge = graph.GetEdge(evaluationNode, destinationNode);
                    if (edge != null)
                    {
                        var newDistance = distances[evaluationNode] + edge.Weight;
                        if (newDistance < distances[destinationNode])
                        {
                            distances[destinationNode] = newDistance;
                            parent[destinationNode] = evaluationNode;
                        }

                        unsettled.Add(destinationNode);
                    }
                }
            }
        }
    }    
}
