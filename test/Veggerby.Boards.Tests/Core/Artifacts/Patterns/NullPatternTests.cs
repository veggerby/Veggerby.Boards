using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns;

public class NullPatternTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize_from_constructor()
        {
            // arrange
            // act
            var actual = new NullPattern();

            // assert
            actual.Should().NotBeNull();
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_equal_same_object()
        {
            // arrange
            var pattern = new NullPattern();

            // act
            var actual = NullPattern.Equals(pattern);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var pattern = new NullPattern();

            // act
            var actual = NullPattern.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_another_any_patterm()
        {
            // arrange
            var pattern1 = new NullPattern();
            var pattern2 = new NullPattern();

            // act
            var actual = NullPattern.Equals(pattern2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_another_pattern()
        {
            // arrange
            var pattern1 = new NullPattern();
            var pattern2 = new DirectionPattern(Direction.North, true);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_other_type()
        {
            // arrange
            var pattern = new NullPattern();

            // act
            var actual = pattern.Equals("some string");

            // assert
            actual.Should().BeFalse();
        }
    }
}