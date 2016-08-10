using System;
using Shouldly;
using Veggerby.Boards.Core.Phases;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Phases
{
    public class RoundTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                
                // act
                var actual = new Round(1);
                
                // assert
                actual.Number.ShouldBe(1);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-3)]
            public void Should_throw_with_zero_or_negative_number(int number)
            {
                // arrange
                
                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => new Round(number));
                
                // assert
                actual.ParamName.ShouldBe("number");
            }
        }

        public class _Equals
        {
            [Fact]
            public void Should_equal_same_object()
            {
                // arrange
                var round = new Round(1);

                // act
                var actual = round.Equals(round);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var round = new Round(1);

                // act
                var actual = round.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_same_number()
            {
                // arrange
                var round1 = new Round(1);
                var round2 = new Round(1);

                // act
                var actual = round1.Equals(round2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_different_number()
            {
                // arrange
                var round1 = new Round(1);
                var round2 = new Round(2);

                // act
                var actual = round1.Equals(round2);

                // assert
                actual.ShouldBeFalse();
            }    
        }
    }
}