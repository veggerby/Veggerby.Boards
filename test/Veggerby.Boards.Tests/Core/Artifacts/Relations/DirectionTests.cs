using Veggerby.Boards.Core.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations
{
    public class DirectionTests
    {
        public class Statics
        {
            [Fact]
            public void Should_contain_static_properties()
            {
                // arrange
                // act
                // assert
                Assert.NotNull(Direction.Left);
                Assert.NotNull(Direction.Right);
                Assert.NotNull(Direction.Up);
                Assert.NotNull(Direction.Down);
                Assert.NotNull(Direction.Across);

                Assert.NotNull(Direction.North);
                Assert.NotNull(Direction.South);
                Assert.NotNull(Direction.East);
                Assert.NotNull(Direction.West);

                Assert.NotNull(Direction.NorthWest);
                Assert.NotNull(Direction.NorthEast);
                Assert.NotNull(Direction.SouthEast);
                Assert.NotNull(Direction.SouthEast);

                Assert.NotNull(Direction.Clockwise);
                Assert.NotNull(Direction.CounterClockwise);
            }
        }

        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                // act
                var actual = new Direction("id");
                
                // assert
                Assert.Equal("id", actual.Id);
            }
        }
    }
}