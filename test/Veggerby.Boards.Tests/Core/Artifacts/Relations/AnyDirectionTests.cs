using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations;

public class AnyDirectionTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize_from_constructor()
        {
            // arrange
            // act
            var actual = new AnyDirection();

            // assert
            actual.Id.Should().Be("any");
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_equal_same_object()
        {
            // arrange
            var direction = new AnyDirection();

            // act
            var actual = direction.Equals(direction);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var direction = new AnyDirection();

            // act
            var actual = direction.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_same_type_same_id()
        {
            // arrange
            var direction1 = new AnyDirection();
            var direction2 = new AnyDirection();

            // act
            var actual = direction1.Equals(direction2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_equal_another_direction()
        {
            // arrange
            var direction1 = new AnyDirection();
            var direction2 = new Direction("dir");

            // act
            var actual = direction1.Equals(direction2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_other_type()
        {
            // arrange
            var direction = new AnyDirection();

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
            var direction = new AnyDirection();

            // act
            var actual = direction.ToString();

            // assert
            actual.Should().Be("AnyDirection any");
        }
    }
}