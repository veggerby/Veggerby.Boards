using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Patterns
{
    public class AnyPatternTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                // act
                var actual = new AnyPattern();

                // assert
                actual.ShouldNotBeNull();
            }
        }

        public class _Equals
        {
            public void Should_equal_same_object()
            {
                // arrange
                var direction = new AnyPattern();

                // act
                var actual = direction.Equals(direction);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var direction = new AnyPattern();

                // act
                var actual = direction.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            public void Should_not_equal_another_pattern()
            {
                // arrange
                var direction1 = new AnyPattern();
                var direction2 = new DirectionPattern(Direction.North, true);

                // act
                var actual = direction1.Equals(direction2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_other_type()
            {
                // arrange
                var direction = new AnyPattern();

                // act
                var actual = direction.Equals("some string");

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}