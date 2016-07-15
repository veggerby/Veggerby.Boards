using Xunit;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Backgammon
{
    public class DoublingCubeTests
    {
        public class GetValue
        {
            [Theory]
            [InlineData(1, 2)]
            [InlineData(2, 4)]
            [InlineData(4, 8)]
            [InlineData(8, 16)]
            [InlineData(16, 32)]
            [InlineData(32, 64)]
            [InlineData(64, 64)]
            public void Should_return_next_doubling_value(int current, int expected)
            {
                // arrange
                var die = new DoublingCube("doubling-cube");
                var currentState = new DieState<int>(die, current);

                // act
                var actual = die.Roll(currentState);

                // asser
                Assert.Equal(expected, actual);
            }
        }
    }
}