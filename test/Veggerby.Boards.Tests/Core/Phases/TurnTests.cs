using System;
using Veggerby.Boards.Core.Phases;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Phases
{
    public class TurnTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_from_constructor()
            {
                // arrange
                var round = new Round(1);

                // act
                var actual = new Turn(round, 1);

                // assert
                Assert.Equal(round, actual.Round);
                Assert.Equal(1, actual.Number);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-3)]
            public void Should_throw_with_zero_or_negative_number(int number)
            {
                // arrange
                var round = new Round(1);

                // act
                var actual = Assert.Throws<ArgumentOutOfRangeException>(() => new Turn(round, number));

                // assert
                Assert.Equal("number", actual.ParamName);
            }

            [Fact]
            public void Should_throw_with_null_round()
            {
                // arrange
                // act
                var actual = Assert.Throws<ArgumentNullException>(() => new Turn(null, 1));

                // assert
                Assert.Equal("round", actual.ParamName);
            }
        }

        public class _Equals
        {
            [Fact]
            public void Should_equal_same_object()
            {
                // arrange
                var round = new Round(1);
                var turn = new Turn(round, 1);

                // act
                var actual = turn.Equals(turn);

                // assert
                Assert.True(actual);
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var round = new Round(1);
                var turn = new Turn(round, 1);

                // act
                var actual = turn.Equals(null);

                // assert
                Assert.False(actual);
            }

            [Fact]
            public void Should_equal_same_number_same_round_object()
            {
                // arrange
                var round = new Round(1);
                var turn1 = new Turn(round, 1);
                var turn2 = new Turn(round, 1);

                // act
                var actual = turn1.Equals(turn2);

                // assert
                Assert.True(actual);
            }


            [Fact]
            public void Should_equal_same_number_same_round_number()
            {
                // arrange
                var round1 = new Round(1);
                var turn1 = new Turn(round1, 1);
                var round2 = new Round(1);
                var turn2 = new Turn(round2, 1);

                // act
                var actual = turn1.Equals(turn2);

                // assert
                Assert.True(actual);
            }

            [Fact]
            public void Should_not_equal_different_number_same_round()
            {
                // arrange
                var round = new Round(1);
                var turn1 = new Turn(round, 1);
                var turn2 = new Turn(round, 2);

                // act
                var actual = turn1.Equals(turn2);

                // assert
                Assert.False(actual);
            }

            [Fact]
            public void Should_not_equal_different_number_different_round()
            {
                // arrange
                var round1 = new Round(1);
                var turn1 = new Turn(round1, 1);
                var round2 = new Round(2);
                var turn2 = new Turn(round2, 2);

                // act
                var actual = turn1.Equals(turn2);

                // assert
                Assert.False(actual);
            }

            [Fact]
            public void Should_not_equal_same_number_different_round()
            {
                // arrange
                var round1 = new Round(1);
                var turn1 = new Turn(round1, 1);
                var round2 = new Round(2);
                var turn2 = new Turn(round2, 1);

                // act
                var actual = turn1.Equals(turn2);

                // assert
                Assert.False(actual);
            }
        }
    }
}