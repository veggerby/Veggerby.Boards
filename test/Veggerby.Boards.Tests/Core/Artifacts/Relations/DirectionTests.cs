using System;

using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations;

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
            Direction.Left.Should().NotBeNull();
            Direction.Right.Should().NotBeNull();
            Direction.Up.Should().NotBeNull();
            Direction.Down.Should().NotBeNull();
            Direction.Across.Should().NotBeNull();

            Direction.North.Should().NotBeNull();
            Direction.South.Should().NotBeNull();
            Direction.East.Should().NotBeNull();
            Direction.West.Should().NotBeNull();

            Direction.NorthWest.Should().NotBeNull();
            Direction.NorthEast.Should().NotBeNull();
            Direction.SouthWest.Should().NotBeNull();
            Direction.SouthEast.Should().NotBeNull();

            Direction.Clockwise.Should().NotBeNull();
            Direction.CounterClockwise.Should().NotBeNull();

            Direction.Any.Should().BeOfType<AnyDirection>();
        }
    }

    public class Create
    {
        [Fact]
        public void Should_initialize_from_constructor()
        {
            // arrange
            // act
            var actual = new Direction("id");

            // assert
            actual.Id.Should().Be("id");
        }

        [Fact]
        public void Should_throw_from_constructor_no_id()
        {
            // arrange
            // act
            var actual = () => new Direction(string.Empty);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("id");
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
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var direction = new Direction("dir");

            // act
            var actual = direction.Equals(null);

            // assert
            actual.Should().BeFalse();
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
            actual.Should().BeTrue();
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
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_any_direction()
        {
            // arrange
            var direction = new Direction("dir1");

            // act
            var actual = direction.Equals(Direction.Any);

            // assert
            actual.Should().BeFalse();
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
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_other_type()
        {
            // arrange
            var direction = new Direction("dir");

            // act
            var actual = direction.Equals("some string");

            // assert
            actual.Should().BeFalse();
        }
    }

    public class _ToString
    {
        [Fact]
        public void Should_return_expected()
        {
            // arrange
            var direction = new Direction("south");

            // act
            var actual = direction.ToString();

            // assert
            actual.Should().Be("Direction south");
        }
    }
}