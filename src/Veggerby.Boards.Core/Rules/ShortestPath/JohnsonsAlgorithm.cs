namespace Veggerby.Boards.Core.Rules.ShortestPath
{
    /// From http://www.sanfoundry.com/java-program-to-implement-johnsons-algorithm/
    public class JohnsonsAlgorithm 
    {
        private int _sourceNode;
        private int _numberOfNodes;
        private int[,] _augmentedMatrix;
        private int[] _potential;
        private BellmanFord _bellmanFord;
        private DijkstraShortestPath _dijsktraShortestPath;
        private int[,] _allPairShortestPath;
    
        public JohnsonsAlgorithm(int numberOfNodes)
        {
            _numberOfNodes = numberOfNodes;
            _augmentedMatrix = new int[numberOfNodes + 2, numberOfNodes + 2];
            _sourceNode = numberOfNodes + 1;
            _potential = new int[numberOfNodes + 2];
            _bellmanFord = new BellmanFord(numberOfNodes + 1);
            _dijsktraShortestPath = new DijkstraShortestPath(numberOfNodes);
            _allPairShortestPath = new int[numberOfNodes + 1, numberOfNodes + 1];
        }
    
        public void johnsonsAlgorithms(int[,] adjacencyMatrix)
        {
            ComputeAugmentedGraph(adjacencyMatrix);
    
            _bellmanFord.BellmanFordEvaluation(_sourceNode, _augmentedMatrix);
            _potential = _bellmanFord.Distances;
    
            int[,] reweightedGraph = ReweightGraph(adjacencyMatrix);
            for (int i = 1; i <= _numberOfNodes; i++)
            {
                for (int j = 1; j <= _numberOfNodes; j++)
                {
                    System.Console.WriteLine(reweightedGraph[i, j] + "\t");
                }

                System.Console.WriteLine();
            }
    
            for (int source = 1; source <= _numberOfNodes; source++)
            {
                _dijsktraShortestPath.dijkstraShortestPath(source, reweightedGraph);
                int[] result = _dijsktraShortestPath.Distances;
                for (int destination = 1; destination <= _numberOfNodes; destination++)
                {
                    _allPairShortestPath[source, destination] = result[destination] 
                            + _potential[destination] - _potential[source];
                }
            }
    
            System.Console.WriteLine();
            for (int i = 1; i<= _numberOfNodes; i++)
            {
                System.Console.WriteLine("\t"+i);
            }

            System.Console.WriteLine();
            for (int source = 1; source <= _numberOfNodes; source++)
            {
                System.Console.WriteLine( source +"\t" );
                for (int destination = 1; destination <= _numberOfNodes; destination++)
                {
                    System.Console.WriteLine(_allPairShortestPath[source, destination]+ "\t");
                }
                System.Console.WriteLine();
            }
        }
    
        private void ComputeAugmentedGraph(int[,] adjacencyMatrix)
        {
            for (int source = 1; source <= _numberOfNodes; source++)
            {
                for (int destination = 1; destination <= _numberOfNodes; destination++)
                { 
                    _augmentedMatrix[source, destination] = adjacencyMatrix[source, destination];
                }
            }

            for (int destination = 1; destination <= _numberOfNodes; destination++)
            {
                _augmentedMatrix[_sourceNode, destination] = 0;
            }
        }
    
        private int[,] ReweightGraph(int[,] adjacencyMatrix)
        {
            int[,] result = new int[_numberOfNodes + 1, _numberOfNodes + 1];
            for (int source = 1; source <= _numberOfNodes; source++)
            {
                for (int destination = 1; destination <= _numberOfNodes; destination++)
                {
                    result[source, destination] = adjacencyMatrix[source, destination]
                        + _potential[source] - _potential[destination];
                }
            }
            return result;
        }
    /*
        public static void main(params string[] args)
        {
            int[,] adjacency_matrix;
            int number_of_vertices;
            Scanner scan = new Scanner(System.in);
    
            try
            {
                System.out.println("Enter the number of vertices");
                number_of_vertices = scan.nextInt();
                adjacency_matrix = new int[number_of_vertices + 1][number_of_vertices + 1];
    
                System.out.println("Enter the Weighted Matrix for the graph");
                for (int i = 1; i <= number_of_vertices; i++)
                {
                    for (int j = 1; j <= number_of_vertices; j++)
                    {
                        adjacency_matrix[i][j] = scan.nextInt();
                        if (i == j) 
                        {
                            adjacency_matrix[i][j] = 0;
                            continue;
                        }
                        if (adjacency_matrix[i][j] == 0)
                        {
                            adjacency_matrix[i][j] = MAX_VALUE;
                        }
                    }
                }
    
                JohnsonsAlgorithm johnsonsAlgorithm = new JohnsonsAlgorithm(number_of_vertices);
                johnsonsAlgorithm.johnsonsAlgorithms(adjacency_matrix);
            } catch (InputMismatchException inputMismatch)
            {
                System.out.println("Wrong Input Format");
            }
            scan.close();
        }
    */
    }
}
