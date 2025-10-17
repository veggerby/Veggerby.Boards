using System;

using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns;

public class DirectionPatternTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize_from_constructor()
        {
            // arrange
            // act
            var actual = new DirectionPattern(Direction.Clockwise);

            // assert
            actual.Direction.Should().Be(Direction.Clockwise);
            actual.IsRepeatable.Should().BeTrue();
        }

        [Fact]
        public void Should_throw_with_null_pattern()
        {
            // arrange
            // act
            var actual = () => new DirectionPattern(null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("direction");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_initialize_from_constructor_with_repeatable_flag(bool isRepeatable)
        {
            // arrange
            // act
            var actual = new DirectionPattern(Direction.Clockwise, isRepeatable);

            // assert
            actual.Direction.Should().Be(Direction.Clockwise);
            actual.IsRepeatable.Should().Be(isRepeatable);
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_equal_same_object()
        {
            // arrange
            var pattern = new DirectionPattern(Direction.North, true);

            // act
            var actual = pattern.Equals(pattern);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var pattern = new DirectionPattern(Direction.North, true);

            // act
            var actual = pattern.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_same_type_same_pattern_and_repeat_state()
        {
            // arrange
            var pattern1 = new DirectionPattern(Direction.North, true);
            var pattern2 = new DirectionPattern(Direction.North, true);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_same_type_same_pattern_and_different_repeat_state()
        {
            // arrange
            var pattern1 = new DirectionPattern(Direction.North, true);
            var pattern2 = new DirectionPattern(Direction.North, false);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_same_type_different_pattern_and_same_repeat_state()
        {
            // arrange
            var pattern1 = new DirectionPattern(Direction.North, true);
            var pattern2 = new DirectionPattern(Direction.South, true);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_another_pattern()
        {
            // arrange
            var pattern1 = new DirectionPattern(Direction.North, true);
            var pattern2 = new AnyPattern();

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_other_type()
        {
            // arrange
            var pattern = new DirectionPattern(Direction.North, true);

            // act
            var actual = pattern.Equals("some string");

            // assert
            actual.Should().BeFalse();
        }
    }
}