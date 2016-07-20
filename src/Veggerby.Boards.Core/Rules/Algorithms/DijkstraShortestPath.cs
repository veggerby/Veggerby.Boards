using System;
using System.Linq;

namespace Veggerby.Boards.Core.Rules.Algorithms
{
    public class DijkstraShortestPath
    {
        public int[] GetShortestPath(int source, int[,] adjacencymatrix)
        {
            if (adjacencymatrix.GetLength(0) != adjacencymatrix.GetLength(1))
            {
                throw new ArgumentException("Adjacency matrix must be square", nameof(adjacencymatrix));
            }
            
            var numberOfVertices = adjacencymatrix.GetLength(0);

            var settled = new bool[numberOfVertices];
            var unsettled = new bool[numberOfVertices];
            var distances = new int[numberOfVertices];
            var _adjacencyMatrix = new int[numberOfVertices, numberOfVertices];
    
            int evaluationnode;
            for (int vertex = 0; vertex < numberOfVertices; vertex++)
            {
                distances[vertex] = int.MaxValue;
            }
    
            for (int sourcevertex = 0; sourcevertex < numberOfVertices; sourcevertex++)
            {
                for (int destinationvertex = 0; destinationvertex < numberOfVertices; destinationvertex++)
                {
                    _adjacencyMatrix[sourcevertex, destinationvertex] = adjacencymatrix[sourcevertex, destinationvertex];
                }
            }
    
            unsettled[source] = true;
            distances[source] = 0;
            while (GetUnsettledCount(unsettled) != 0)
            {
                evaluationnode = GetNodeWithMinimumDistanceFromUnsettled(unsettled, distances);
                unsettled[evaluationnode] = false;
                settled[evaluationnode] = true;

                EvaluateNeighbours(evaluationnode, settled, _adjacencyMatrix, ref unsettled, ref distances);
            }

            return distances;
        } 
    
        private int GetUnsettledCount(bool[] unsettled)
        {
            return unsettled.Count(x => x);
        }
    
        private int GetNodeWithMinimumDistanceFromUnsettled(bool[] unsettled, int[] distances)
        {
            int min = int.MaxValue;
            int node = 0;
            for (int vertex = 0; vertex < unsettled.Length; vertex++)
            {
                if (unsettled[vertex] == true && distances[vertex] < min)
                {
                    node = vertex;
                    min = distances[vertex];
                }
            }

            return node;
        }
    
        private void EvaluateNeighbours(int evaluationNode, bool[] settled, int[,] adjacencyMatrix, ref bool[] unsettled, ref int[] distances)
        {
            int edgeDistance = -1;
            int newDistance = -1;
    
            for (int destinationNode = 0; destinationNode < settled.Length; destinationNode++)
            {
                if (settled[destinationNode] == false)
                {
                    if (adjacencyMatrix[evaluationNode, destinationNode] != int.MaxValue)
                    {
                        edgeDistance = adjacencyMatrix[evaluationNode, destinationNode];
                        newDistance = distances[evaluationNode] + edgeDistance;
                        if (newDistance < distances[destinationNode])
                        {
                            distances[destinationNode] = newDistance;
                        }

                        unsettled[destinationNode] = true;
                    }
                }
            }
        }
    }    
}
