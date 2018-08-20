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
            public void Should_throw_with_null_pattern()
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
            [Fact]
            public void Should_equal_same_object()
            {
                // arrange
                var pattern = new DirectionPattern(Direction.North, true);

                // act
                var actual = pattern.Equals(pattern);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var pattern = new DirectionPattern(Direction.North, true);

                // act
                var actual = pattern.Equals(null);

                // assert
                actual.ShouldBeFalse();
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
                actual.ShouldBeTrue();
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
                actual.ShouldBeFalse();
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
                actual.ShouldBeFalse();
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
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_other_type()
            {
                // arrange
                var pattern = new DirectionPattern(Direction.North, true);

                // act
                var actual = pattern.Equals("some string");

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}