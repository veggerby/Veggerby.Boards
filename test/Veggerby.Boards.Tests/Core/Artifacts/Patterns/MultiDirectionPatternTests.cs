using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns
{
    public class MultiDirectionPatternTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                // act
                var actual = new MultiDirectionPattern(new[] { Direction.Clockwise, Direction.CounterClockwise });

                // assert
                actual.Directions.ShouldBe(new[] { Direction.Clockwise, Direction.CounterClockwise });
                actual.IsRepeatable.ShouldBeTrue();
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void Should_initialize_from_constructor_with_repeatable_flag(bool isRepeatable)
            {
                // arrange
                // act
                var actual = new MultiDirectionPattern(new[] { Direction.Clockwise, Direction.CounterClockwise }, isRepeatable);

                // assert
                actual.Directions.ShouldBe(new[] { Direction.Clockwise, Direction.CounterClockwise });
                actual.IsRepeatable.ShouldBe(isRepeatable);
            }

            [Fact]
            public void Should_throw_with_null_patterns()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentNullException>(() => new MultiDirectionPattern(null));

                // assert
                actual.ParamName.ShouldBe("directions");
            }

            [Fact]
            public void Should_throw_with_empty_patterns()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentException>(() => new MultiDirectionPattern(Enumerable.Empty<Direction>()));

                // assert
                actual.ParamName.ShouldBe("directions");
            }
        }

        public class _Equals
        {
            [Fact]
            public void Should_equal_same_object()
            {
                // arrange
                var pattern = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });

                // act
                var actual = pattern.Equals(pattern);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var pattern = new MultiDirectionPattern(new[] { Direction.South });

                // act
                var actual = pattern.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_same_pattern()
            {
                // arrange
                var pattern1 = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });
                var pattern2 = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });

                // act
                var actual = pattern1.Equals(pattern2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_equal_pattern_same_directions_different_order()
            {
                // arrange
                var pattern1 = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });
                var pattern2 = new MultiDirectionPattern(new [] { Direction.East, Direction.North, Direction.North });

                // act
                var actual = pattern1.Equals(pattern2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_pattern_same_not_repeatable()
            {
                // arrange
                var pattern1 = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });
                var pattern2 = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East }, false);

                // act
                var actual = pattern1.Equals(pattern2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_another_pattern()
            {
                // arrange
                var pattern1 = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });
                var pattern2 = new DirectionPattern(Direction.North, true);

                // act
                var actual = pattern1.Equals(pattern2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_other_type()
            {
                // arrange
                var pattern = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });

                // act
                var actual = pattern.Equals("some string");

                // assert
                actual.ShouldBeFalse();
            }
        }

        public class _GetHashCode
        {
            [Fact]
            public void Should_return_hashcode()
            {
                // arrange
                var expected = true.GetHashCode() ^
                    Direction.North.GetHashCode() ^
                    Direction.North.GetHashCode() ^
                    Direction.East.GetHashCode();

                var pattern = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });

                // act
                var actual = pattern.GetHashCode();

                // assert
                actual.ShouldBe(expected);
            }

            [Fact]
            public void Should_equal_hashcode_same_pattern_different_order()
            {
                // arrange
                var pattern1 = new MultiDirectionPattern(new [] { Direction.North, Direction.North, Direction.East });
                var pattern2 = new MultiDirectionPattern(new [] { Direction.East, Direction.North, Direction.North });

                // act
                var h1 = pattern1.GetHashCode();
                var h2 = pattern2.GetHashCode();
                var actual = h1 == h2;

                // assert
                actual.ShouldBeTrue();
            }
        }
    }
}