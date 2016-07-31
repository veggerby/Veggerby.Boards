using System;
using System.Linq;
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
                Assert.Equal(new[] { Direction.Clockwise, Direction.CounterClockwise }, actual.Directions);
                Assert.True(actual.IsRepeatable);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void Should_initialize_from_constructor(bool isRepeatable)
            {
                // arrange
                // act
                var actual = new MultiDirectionPattern(new[] { Direction.Clockwise, Direction.CounterClockwise }, isRepeatable);
                
                // assert
                Assert.Equal(new[] { Direction.Clockwise, Direction.CounterClockwise }, actual.Directions);
                Assert.Equal(isRepeatable, actual.IsRepeatable);
            }

            [Fact]
            public void Should_throw_with_null_patterns()
            {
                // arrange
                // act
                var actual = Assert.Throws<ArgumentNullException>(() => new MultiDirectionPattern(null));
                
                // assert
                Assert.Equal("directions", actual.ParamName);
            }

            [Fact]
            public void Should_throw_with_empty_patterns()
            {
                // arrange
                // act
                var actual = Assert.Throws<ArgumentException>(() => new MultiDirectionPattern(Enumerable.Empty<Direction>()));
                
                // assert
                Assert.Equal("directions", actual.ParamName);
            }
        }
    }
}