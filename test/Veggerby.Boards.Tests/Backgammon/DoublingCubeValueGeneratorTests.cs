using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Utils;
using Xunit;

namespace Veggerby.Boards.Tests.Backgammon
{
    public class DoublingCubeValueGeneratorTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize()
            {
                // arrange

                // act
                var actual = new DoublingDiceValueGenerator();

                // assert
                actual.MinValue.ShouldBe(1);
                actual.MaxValue.ShouldBe(64);
            }
        }

        public class GetValue
        {
            [Theory]
            [InlineData(1, 2)]
            [InlineData(2, 4)]
            [InlineData(4, 8)]
            [InlineData(8, 16)]
            [InlineData(16, 32)]
            [InlineData(32, 64)]
            [InlineData(64, 64)]
            public void Should_return_doubling_value(int initialValue, int expected)
            {
                // arrange
                var generator = new DoublingDiceValueGenerator();
                var currentState = new DiceState<int>(new Dice("dice"), initialValue);

                // act
                var actual = generator.GetValue(currentState);

                // assert
                actual.ShouldBe(expected);
            }

            [Fact]
            public void Should_throw_with_null_state()
            {
                // arrange
                var generator = new DoublingDiceValueGenerator();

                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => generator.GetValue(null));

                // assert
                actual.ParamName.ShouldBe("currentState");
            }

            [Fact]
            public void Should_throw_with_different_state_type()
            {
                // arrange
                var generator = new DoublingDiceValueGenerator();
                var initialState = new NullDiceState(new Dice("dice"));

                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => generator.GetValue(initialState));

                // assert
                actual.ParamName.ShouldBe("currentState");
            }

            [Fact]
            public void Should_throw_with_value_below_min()
            {
                // arrange
                var generator = new DoublingDiceValueGenerator();
                var initialState = new DiceState<int>(new Dice("dice"), -1);

                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => generator.GetValue(initialState));

                // assert
                actual.ParamName.ShouldBe("currentState");
            }

            [Fact]
            public void Should_throw_not_a_factor_of_2()
            {
                // arrange
                var generator = new DoublingDiceValueGenerator();
                var initialState = new DiceState<int>(new Dice("dice"), 15);

                // act
                var actual = Should.Throw<ArgumentOutOfRangeException>(() => generator.GetValue(initialState));

                // assert
                actual.ParamName.ShouldBe("currentState");
            }
        }
    }
}