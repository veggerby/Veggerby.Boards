using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Events
{
    public class RollDiceGameEventTests
    {
        public class ctor
        {
            [Fact]
            public void Should_create_event()
            {
                // arrange
                var dice = new RegularDice("dice");
                var newState = new DiceState<int>(dice, 4);

                // act
                var actual = new RollDiceGameEvent<int>(newState);

                // assert
                actual.NewDiceStates.ShouldBe(new [] { newState });
            }

            [Fact]
            public void Should_throw_with_null_dice()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new RollDiceGameEvent<int>(null));

                // assert
                actual.ParamName.ShouldBe("states");
            }

            [Fact]
            public void Should_throw_with_empty_dice()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new RollDiceGameEvent<string>());

                // assert
                actual.ParamName.ShouldBe("states");
            }
        }
    }
}