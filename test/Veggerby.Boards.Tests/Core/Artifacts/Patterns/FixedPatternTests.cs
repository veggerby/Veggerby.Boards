using System;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns
{
    public class FixedPatternTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                // act
                var actual = new FixedPattern(new [] { Direction.Clockwise, Direction.Clockwise, Direction.Across });
                
                // assert
                Assert.Equal(new [] { Direction.Clockwise, Direction.Clockwise, Direction.Across }, actual.Pattern);
            }

            [Fact]
            public void Should_throw_with_null_patterns()
            {
                // arrange
                // act
                var actual = Assert.Throws<ArgumentNullException>(() => new FixedPattern(null));
                
                // assert
                Assert.Equal("pattern", actual.ParamName);
            }

            [Fact]
            public void Should_throw_with_empty_patterns()
            {
                // arrange
                // act
                var actual = Assert.Throws<ArgumentException>(() => new FixedPattern(Enumerable.Empty<Direction>()));
                
                // assert
                Assert.Equal("pattern", actual.ParamName);
            }
        }
    }
}