using System;

namespace Veggerby.Boards.Core.Rules.Algorithms
{
    /// From http://www.sanfoundry.com/java-program-to-implement-johnsons-algorithm/
    public class JohnsonsAlgorithm 
    {
        public int[,] GetShortestPath(int[,] adjacencyMatrix)
        {
            if (adjacencyMatrix.GetLength(0) != adjacencyMatrix.GetLength(1))
            {
                throw new ArgumentException("Adjacency matrix must be square", nameof(adjacencyMatrix));
            }
            
            var numberOfNodes = adjacencyMatrix.GetLength(0);
            var sourceNode = numberOfNodes;

            var bellmanFord = new BellmanFord();
            var dijsktraShortestPath = new DijkstraShortestPath();

            var augmentedMatrix = ComputeAugmentedGraph(adjacencyMatrix, sourceNode);

            var potential = bellmanFord.BellmanFordEvaluation(sourceNode, augmentedMatrix);
            var reweightedGraph = ReweightGraph(adjacencyMatrix, potential);
            var allPairShortestPath = new int[numberOfNodes, numberOfNodes];     

            for (int source = 0; source < numberOfNodes; source++)
            {
                var result = dijsktraShortestPath.GetShortestPath(source, reweightedGraph);

                for (int destination = 0; destination < numberOfNodes; destination++)
                {
                    allPairShortestPath[source, destination] = result[destination] + potential[destination] - potential[source];
                }
            }

            return allPairShortestPath;
        }
    
        private int[,] ComputeAugmentedGraph(int[,] adjacencyMatrix, int sourceNode)
        {
            var numberOfNodes = adjacencyMatrix.GetLength(0);
            
            var augmentedMatrix = new int[numberOfNodes + 1, numberOfNodes + 1];
            
            for (int source = 0; source < numberOfNodes; source++)
            {
                for (int destination = 0; destination < numberOfNodes; destination++)
                { 
                    augmentedMatrix[source, destination] = adjacencyMatrix[source, destination];
                }
            }

            for (int destination = 0; destination < numberOfNodes; destination++)
            {
                augmentedMatrix[sourceNode, destination] = 0;
            }

            return augmentedMatrix;
        }
    
        private int[,] ReweightGraph(int[,] adjacencyMatrix, int[] potential)
        {
            var numberOfNodes = adjacencyMatrix.GetLength(0);

            int[,] result = new int[numberOfNodes, numberOfNodes];
            for (int source = 0; source < numberOfNodes; source++)
            {
                for (int destination = 0; destination < numberOfNodes; destination++)
                {
                    result[source, destination] = adjacencyMatrix[source, destination] + potential[source] - potential[destination];
                }
            }

            return result;
        }
    }
}
