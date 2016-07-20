using System;

namespace Veggerby.Boards.Core.Rules.Algorithms
{
    public class BellmanFord 
    {
        public int[] BellmanFordEvaluation(int source, int[,] adjacencymatrix)
        {
            if (adjacencymatrix.GetLength(0) != adjacencymatrix.GetLength(1))
            {
                throw new ArgumentException("Adjacency matrix must be square", nameof(adjacencymatrix));
            }
            
            var numberOfVertices = adjacencymatrix.GetLength(0);

            var distances = new int[numberOfVertices];

            for (int node = 0; node < numberOfVertices; node++)
            {
                distances[node] = int.MaxValue;
            }
    
            distances[source] = 0;
    
            for (int node = 0; node < numberOfVertices - 1; node++)
            {
                for (int sourcenode = 0; sourcenode < numberOfVertices; sourcenode++)
                {
                    for (int destinationnode = 0; destinationnode < numberOfVertices; destinationnode++)
                    {
                        if (adjacencymatrix[sourcenode, destinationnode] != int.MaxValue)
                        {
                            if (distances[destinationnode] > distances[sourcenode] + adjacencymatrix[sourcenode, destinationnode])
                            {
                                distances[destinationnode] = distances[sourcenode] + adjacencymatrix[sourcenode, destinationnode];
                            }
                        }
                    }
                }
            }
    
            for (int sourcenode = 0; sourcenode < numberOfVertices; sourcenode++)
            {
                for (int destinationnode = 0; destinationnode < numberOfVertices; destinationnode++)
                {
                    if (adjacencymatrix[sourcenode, destinationnode] != int.MaxValue)
                    {
                        if (distances[destinationnode] > distances[sourcenode] + adjacencymatrix[sourcenode, destinationnode])
                        {
                            throw new BoardException("The Graph contains negative egde cycle");
                        }
                    }
                }
            }

            return distances;
        }
    }
}
