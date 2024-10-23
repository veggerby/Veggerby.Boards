using System;
using System.Linq;

using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns;

public class FixedPatternTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize_from_constructor()
        {
            // arrange
            // act
            var actual = new FixedPattern([Direction.Clockwise, Direction.Clockwise, Direction.Across]);

            // assert
            actual.Pattern.Should().Equal([Direction.Clockwise, Direction.Clockwise, Direction.Across]);
        }

        [Fact]
        public void Should_throw_with_null_patterns()
        {
            // arrange
            // act
            var actual = () => new FixedPattern(null);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("pattern");
        }

        [Fact]
        public void Should_throw_with_empty_patterns()
        {
            // arrange
            // act
            var actual = () => new FixedPattern(Enumerable.Empty<Direction>());

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("pattern");
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_equal_same_object()
        {
            // arrange
            var pattern = new FixedPattern([Direction.North, Direction.North, Direction.East]);

            // act
            var actual = pattern.Equals(pattern);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var pattern = new FixedPattern([Direction.South]);

            // act
            var actual = pattern.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_same_pattern()
        {
            // arrange
            var pattern1 = new FixedPattern([Direction.North, Direction.North, Direction.East]);
            var pattern2 = new FixedPattern([Direction.North, Direction.North, Direction.East]);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_pattern_same_directions_different_order()
        {
            // arrange
            var pattern1 = new FixedPattern([Direction.North, Direction.North, Direction.East]);
            var pattern2 = new FixedPattern([Direction.East, Direction.North, Direction.North]);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_another_pattern()
        {
            // arrange
            var pattern1 = new FixedPattern([Direction.North, Direction.North, Direction.East]);
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
            var pattern = new FixedPattern([Direction.North, Direction.North, Direction.East]);

            // act
            var actual = pattern.Equals("some string");

            // assert
            actual.Should().BeFalse();
        }
    }
}