using Veggerby.Boards.Core.Rules.Algorithms;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Rules.Algorithms
{
    public class JohnsonsAlgorithmTests
    {
        public class GetShortestPath
        {
            [Fact]
            public void Should_get_shortest_path()
            {
                // arrange
                var algorithm = new JohnsonsAlgorithm();

                var matrix = new int[,] 
                {
                    { 0, int.MaxValue, 3, int.MaxValue },
                    { 2, 0, int.MaxValue, int.MaxValue },
                    { int.MaxValue, 7, 0, 1 },
                    { 6, int.MaxValue, int.MaxValue, 0 }   
                };
                
                var expected = new int[,]
                {
                    { 0, 10, 3, 4 },
                    { 2, 0, 5, 6 },
                    { 7, 7, 0, 1 },
                    { 6, 16, 9, 0 }
                };
                
                // act
                var actual = algorithm.GetShortestPath(matrix);
                
                // assert
                Assert.Equal(expected, actual);
            }
        }
    }
}