using System;
using System.Linq;

using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns;

public class MultiDirectionPatternTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize_from_constructor()
        {
            // arrange
            // act
            var actual = new MultiDirectionPattern([Direction.Clockwise, Direction.CounterClockwise]);

            // assert
            actual.Directions.Should().Equal([Direction.Clockwise, Direction.CounterClockwise]);
            actual.IsRepeatable.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_initialize_from_constructor_with_repeatable_flag(bool isRepeatable)
        {
            // arrange
            // act
            var actual = new MultiDirectionPattern([Direction.Clockwise, Direction.CounterClockwise], isRepeatable);

            // assert
            actual.Directions.Should().Equal([Direction.Clockwise, Direction.CounterClockwise]);
            actual.IsRepeatable.Should().Be(isRepeatable);
        }

        [Fact]
        public void Should_throw_with_null_patterns()
        {
            // arrange
            // act
            var actual = () => new MultiDirectionPattern(null);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("directions");
        }

        [Fact]
        public void Should_throw_with_empty_patterns()
        {
            // arrange
            // act
            var actual = () => new MultiDirectionPattern(Enumerable.Empty<Direction>());

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("directions");
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_equal_same_object()
        {
            // arrange
            var pattern = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East]);

            // act
            var actual = pattern.Equals(pattern);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var pattern = new MultiDirectionPattern([Direction.South]);

            // act
            var actual = pattern.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_same_pattern()
        {
            // arrange
            var pattern1 = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East]);
            var pattern2 = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East]);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_equal_pattern_same_directions_different_order()
        {
            // arrange
            var pattern1 = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East]);
            var pattern2 = new MultiDirectionPattern([Direction.East, Direction.North, Direction.North]);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_pattern_same_not_repeatable()
        {
            // arrange
            var pattern1 = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East]);
            var pattern2 = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East], false);

            // act
            var actual = pattern1.Equals(pattern2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_another_pattern()
        {
            // arrange
            var pattern1 = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East]);
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
            var pattern = new MultiDirectionPattern([Direction.North, Direction.North, Direction.East]);

            // act
            var actual = pattern.Equals("some string");

            // assert
            actual.Should().BeFalse();
        }
    }
}