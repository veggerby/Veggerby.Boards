using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Rules.Algorithms
{
    public class DijkstraShortestPath
    {
        public IDictionary<T, int> GetShortestPath<T>(T source, Graph<T> graph)
        {
            var settled = new List<T>();
            var unsettled = new List<T>(graph.Vertices);
            var distances = graph.Vertices.ToDictionary(x => x, x => source.Equals(x) ? 0 : int.MaxValue);

            while (unsettled.Any())
            {
                var evaluationnode = GetNodeWithMinimumDistanceFromUnsettled(unsettled, distances);
                unsettled.Remove(evaluationnode);
                settled.Add(evaluationnode);
                EvaluateNeighbours(evaluationnode, settled, graph, unsettled, distances);
            }

            return distances;
        }

        private T GetNodeWithMinimumDistanceFromUnsettled<T>(IEnumerable<T> unsettled, IDictionary<T, int> distances)
        {
            return unsettled
                .OrderBy(x => distances.ContainsKey(x) ? distances[x] : int.MaxValue)
                .FirstOrDefault();
        }
    
        private void EvaluateNeighbours<T>(T evaluationNode, IEnumerable<T> settled, Graph<T> graph, IList<T> unsettled, IDictionary<T, int> distances)
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
                        }

                        unsettled.Add(destinationNode);
                    }
                }
            }
        }
    }    
}
