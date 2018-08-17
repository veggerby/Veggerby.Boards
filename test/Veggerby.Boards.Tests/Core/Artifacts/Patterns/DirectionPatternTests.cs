using System;
using Shouldly;
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
                actual.Direction.ShouldBe(Direction.Clockwise);
                actual.IsRepeatable.ShouldBeTrue();
            }

            [Fact]
            public void Should_throw_with_null_direction()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentNullException>(() => new DirectionPattern(null));

                // assert
                actual.ParamName.ShouldBe("direction");
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
                actual.Direction.ShouldBe(Direction.Clockwise);
                actual.IsRepeatable.ShouldBe(isRepeatable);
            }
        }

        public class _Equals
        {
            public void Should_equal_same_object()
            {
                // arrange
                var direction = new DirectionPattern(Direction.North, true);

                // act
                var actual = direction.Equals(direction);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var direction = new DirectionPattern(Direction.North, true);

                // act
                var actual = direction.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_same_type_same_direction_and_repeat_state()
            {
                // arrange
                var direction1 = new DirectionPattern(Direction.North, true);
                var direction2 = new DirectionPattern(Direction.North, true);

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_same_type_same_direction_and_different_repeat_state()
            {
                // arrange
                var direction1 = new DirectionPattern(Direction.North, true);
                var direction2 = new DirectionPattern(Direction.North, false);

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_same_type_different_direction_and_same_repeat_state()
            {
                // arrange
                var direction1 = new DirectionPattern(Direction.North, true);
                var direction2 = new DirectionPattern(Direction.South, true);

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_another_pattern()
            {
                // arrange
                var direction1 = new DirectionPattern(Direction.North, true);
                var direction2 = new AnyPattern();

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_other_type()
            {
                // arrange
                var direction = new DirectionPattern(Direction.North, true);

                // act
                var actual = direction.Equals("some string");

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}