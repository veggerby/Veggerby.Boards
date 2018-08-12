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
    }
}