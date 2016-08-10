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
            public void Should_initialize_from_constructor(bool isRepeatable)
            {
                // arrange
                // act
                var actual = new DirectionPattern(Direction.Clockwise, isRepeatable);
                
                // assert
                actual.Direction.ShouldBe(Direction.Clockwise);
                actual.IsRepeatable.ShouldBe(isRepeatable);
            }
        }
    }
}