using System;
using System.Linq;
using Shouldly;
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
                actual.Pattern.ShouldBe(new [] { Direction.Clockwise, Direction.Clockwise, Direction.Across });
            }

            [Fact]
            public void Should_throw_with_null_patterns()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentNullException>(() => new FixedPattern(null));
                
                // assert
                 actual.ParamName.ShouldBe("pattern");
            }

            [Fact]
            public void Should_throw_with_empty_patterns()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentException>(() => new FixedPattern(Enumerable.Empty<Direction>()));
                
                // assert
                actual.ParamName.ShouldBe("pattern");
            }
        }
    }
}