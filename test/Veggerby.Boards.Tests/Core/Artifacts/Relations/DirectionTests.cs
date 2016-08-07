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

        public class _Equals
        {
            [Fact]
            public void Should_equal_same_object()
            {
                // arrange
                var direction = new Direction("dir");

                // act
                var actual = direction.Equals(direction);

                // assert
                Assert.True(actual);
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var direction = new Direction("dir");

                // act
                var actual = direction.Equals(null);

                // assert
                Assert.False(actual);
            }

            [Fact]
            public void Should_equal_same_type_same_id()
            {
                // arrange
                var direction1 = new Direction("dir");
                var direction2 = new Direction("dir");

                // act
                var actual = direction1.Equals(direction2);

                // assert
                Assert.True(actual);
            }

            [Fact]
            public void Should_not_equal_same_type_different_id()
            {
                // arrange
                var direction1 = new Direction("dir1");
                var direction2 = new Direction("dir2");

                // act
                var actual = direction1.Equals(direction2);

                // assert
                Assert.False(actual);
            }

            [Fact]
            public void Should_equal_any_direction()
            {
                // arrange
                var direction = new Direction("dir1");

                // act
                var actual = direction.Equals(Direction.Any);

                // assert
                Assert.True(actual);
            }

            [Fact]
            public void Should_equal_any_other_direction()
            {
                // arrange
                var direction1 = new AnyDirection();
                var direction2 = new Direction("dir2");

                // act
                var actual = direction1.Equals(direction2);

                // assert
                Assert.True(actual);
            }
        }
    }
}