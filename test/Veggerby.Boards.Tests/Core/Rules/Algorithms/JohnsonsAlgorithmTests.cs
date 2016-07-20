using System.Linq;
using Veggerby.Boards.Core.Rules.Algorithms;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Rules.Algorithms
{
    public class JohnsonsAlgorithmTests
    {
        public class GetShortestPath
        {
            [Fact]
            public void Should_get_shortest_path_sample1()
            {
                /*
                    based on example from: http://www.sanfoundry.com/java-program-to-implement-johnsons-algorithm/

                    Weighted Matrix for the graph
                    0 0 3 0
                    2 0 0 0
                    0 7 0 1
                    6 0 0 0
                    
                    All pair shortest path is 
                        1	2	3	4
                    1	0	10	3	4	
                    2	2	0	5	6	
                    3	7	7	0	1	
                    4	6	16	9	0
                */

                // arrange
                var algorithm = new JohnsonsAlgorithm();

                var graph = new Graph<int>(
                    Enumerable.Range(0, 4),
                    new [] {
                        new Edge<int>(0, 2, 3), // 0 -> 2 = 3
                        new Edge<int>(1, 0, 2), // 1 -> 0 = 2
                        new Edge<int>(2, 1, 7), // 2 -> 1 = 7
                        new Edge<int>(2, 3, 1), // 2 -> 3 = 1
                        new Edge<int>(3, 0, 6), // 3 -> 0 = 6
                    });
                
                var expected = new [] {
                    new Edge<int>(0, 1, 10),
                    new Edge<int>(0, 2, 3),
                    new Edge<int>(0, 3, 4),
                    new Edge<int>(1, 0, 2),
                    new Edge<int>(1, 2, 5),
                    new Edge<int>(1, 3, 6),
                    new Edge<int>(2, 0, 7),
                    new Edge<int>(2, 1, 7),
                    new Edge<int>(2, 3, 1),
                    new Edge<int>(3, 0, 6),
                    new Edge<int>(3, 1, 16),
                    new Edge<int>(3, 2, 9),
                };
                
                // act
                var actual = algorithm.GetShortestPath(graph, int.MaxValue);

                // assert
                Assert.Equal(expected, actual.Select(x => new Edge<int>(x.From, x.To, x.Distance)));
            }
        }
    }
}