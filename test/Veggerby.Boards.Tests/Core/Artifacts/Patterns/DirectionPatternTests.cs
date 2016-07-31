using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns
{
    public class DirectionPatternTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                // act
                var actual = new DirectionPattern(Direction.Clockwise);
                
                // assert
                Assert.Equal(Direction.Clockwise, actual.Direction);
                Assert.True(actual.IsRepeatable);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void Should_initialize_from_constructor(bool isRepeatable)
            {
                // arrange
                // act
                var actual = new DirectionPattern(Direction.Clockwise, isRepeatable);
                
                // assert
                Assert.Equal(Direction.Clockwise, actual.Direction);
                Assert.Equal(isRepeatable, actual.IsRepeatable);
            }
        }
    }
}