using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Flows.Events;

public class RollDiceGameEventTests
{
    public class Create
    {
        [Fact]
        public void Should_create_event()
        {
            // arrange
            var dice = new Dice("dice");
            var newState = new DiceState<int>(dice, 4);

            // act
            var actual = new RollDiceGameEvent<int>(newState);

            // assert
            actual.NewDiceStates.Should().Equal([newState]);
        }

        [Fact]
        public void Should_throw_with_null_dice()
        {
            // arrange

            // act
            var actual = () => new RollDiceGameEvent<int>(null);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("states");
        }

        [Fact]
        public void Should_throw_with_empty_dice()
        {
            // arrange

            // act
            var actual = () => new RollDiceGameEvent<string>();

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("states");
        }
    }
}