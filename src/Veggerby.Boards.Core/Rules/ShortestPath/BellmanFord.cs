namespace Veggerby.Boards.Core.Rules.ShortestPath
{
    public class BellmanFord 
    {
        private int _numberOfVertices;
    
        public int[] Distances { get; private set;}

        public BellmanFord(int numberofvertices)  
        {
            _numberOfVertices = numberofvertices;
            Distances = new int[numberofvertices + 1];
        }
    
        public void BellmanFordEvaluation(int source, int[,] adjacencymatrix)
        {
            for (int node = 1; node <= _numberOfVertices; node++)
            {
                Distances[node] = int.MaxValue;
            }
    
            Distances[source] = 0;
    
            for (int node = 1; node <= _numberOfVertices - 1; node++)
            {
                for (int sourcenode = 1; sourcenode <= _numberOfVertices; sourcenode++)
                {
                    for (int destinationnode = 1; destinationnode <= _numberOfVertices; destinationnode++)
                    {
                        if (adjacencymatrix[sourcenode, destinationnode] != int.MaxValue)
                        {
                            if (Distances[destinationnode] > Distances[sourcenode] 
                                + adjacencymatrix[sourcenode, destinationnode])
                            {
                                Distances[destinationnode] = Distances[sourcenode]
                                + adjacencymatrix[sourcenode, destinationnode];
                            }
                        }
                    }
                }
            }
    
            for (int sourcenode = 1; sourcenode <= _numberOfVertices; sourcenode++)
            {
                for (int destinationnode = 1; destinationnode <= _numberOfVertices; destinationnode++)
                {
                    if (adjacencymatrix[sourcenode, destinationnode] != int.MaxValue)
                    {
                        if (Distances[destinationnode] > Distances[sourcenode] + adjacencymatrix[sourcenode, destinationnode])
                            System.Console.WriteLine("The Graph contains negative egde cycle");
                }
                }
            }
        }
    }
}
