using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns
{
    public class FixedPatternTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                // act
                var actual = new FixedPattern(new [] { Direction.Clockwise, Direction.Clockwise, Direction.Across });
                
                // assert
                Assert.Equal(new [] { Direction.Clockwise, Direction.Clockwise, Direction.Across }, actual.Pattern);
            }
        }
    }
}