using System;
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
                Assert.Equal(1, actual.Number);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-3)]
            public void Should_throw_with_zero_or_negative_number(int number)
            {
                // arrange
                
                // act
                var actual = Assert.Throws<ArgumentOutOfRangeException>(() => new Round(number));
                
                // assert
                Assert.Equal("number", actual.ParamName);
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
                Assert.True(actual);
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var round = new Round(1);

                // act
                var actual = round.Equals(null);

                // assert
                Assert.False(actual);
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
                Assert.True(actual);
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
                Assert.False(actual);
            }    
        }
    }
}