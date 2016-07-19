namespace Veggerby.Boards.Core.Rules.ShortestPath
{
    public class DijkstraShortestPath
    {
        private bool[] _settled;
        private bool[] _unsettled;
        private int[,] _adjacencyMatrix;
        private int _numberOfVertices;

        public int[] Distances { get; private set;}
    
        public DijkstraShortestPath(int numberofvertices)
        {
            _numberOfVertices = numberofvertices;
        }
    
        public void dijkstraShortestPath(int source, int[,] adjacencymatrix)
        {
            _settled = new bool[_numberOfVertices + 1];
            _unsettled = new bool[_numberOfVertices + 1];
            Distances = new int[_numberOfVertices + 1];
            _adjacencyMatrix = new int[_numberOfVertices + 1, _numberOfVertices + 1];
    
            int evaluationnode;
            for (int vertex = 1; vertex <= _numberOfVertices; vertex++)
            {
                Distances[vertex] = int.MaxValue;
            }
    
            for (int sourcevertex = 1; sourcevertex <= _numberOfVertices; sourcevertex++)
            {
                for (int destinationvertex = 1; destinationvertex <= _numberOfVertices; destinationvertex++)
                {
                    _adjacencyMatrix[sourcevertex, destinationvertex] 
                        = adjacencymatrix[sourcevertex, destinationvertex];
                }
            }
    
            _unsettled[source] = true;
            Distances[source] = 0;
            while (getUnsettledCount(_unsettled) != 0)
            {
                evaluationnode = getNodeWithMinimumDistanceFromUnsettled(_unsettled);
                _unsettled[evaluationnode] = false;
                _settled[evaluationnode] = true;
                evaluateNeighbours(evaluationnode);
            }
        } 
    
        public int getUnsettledCount(bool[] unsettled)
        {
            int count = 0;
            for (int vertex = 1; vertex <= _numberOfVertices; vertex++)
            {
                if (unsettled[vertex] == true)
                {
                    count++;
                }
            }
            return count;
        }
    
        public int getNodeWithMinimumDistanceFromUnsettled(bool[] unsettled)
        {
            int min = int.MaxValue;
            int node = 0;
            for (int vertex = 1; vertex <= _numberOfVertices; vertex++)
            {
                if (unsettled[vertex] == true && Distances[vertex] < min)
                {
                    node = vertex;
                    min = Distances[vertex];
                }
            }
            return node;
        }
    
        public void evaluateNeighbours(int evaluationNode)
        {
            int edgeDistance = -1;
            int newDistance = -1;
    
            for (int destinationNode = 1; destinationNode <= _numberOfVertices; destinationNode++)
            {
                if (_settled[destinationNode] == false)
                {
                    if (_adjacencyMatrix[evaluationNode, destinationNode] != int.MaxValue)
                    {
                        edgeDistance = _adjacencyMatrix[evaluationNode, destinationNode];
                        newDistance = Distances[evaluationNode] + edgeDistance;
                        if (newDistance < Distances[destinationNode])
                        {
                            Distances[destinationNode] = newDistance;
                        }
                        _unsettled[destinationNode] = true;
                    }
                }
            }
        }
    }    
}
