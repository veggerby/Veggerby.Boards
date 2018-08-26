using Shouldly;
using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States
{
    public class DiceStateTests
    {
        public class ctor
        {
            [Fact]
            public void Should_create_dice_state()
            {
                // assert
                var dice = new Dice<int>("dice");

                // act
                var actual = new DiceState<int>(dice, 5);

                // assert
                actual.Artifact.ShouldBe(dice);
                actual.CurrentValue.ShouldBe(5);
            }

            [Fact]
            public void Should_throw_when_null_dice()
            {
                // assert

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new DiceState<int>(null, 5));

                // assert
                actual.ParamName.ShouldBe("artifact");
            }

            [Fact]
            public void Should_throw_when_null_value()
            {
                // assert
                var dice = new Dice<string>("dice");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new DiceState<string>(dice, null));

                // assert
                actual.ParamName.ShouldBe("currentValue");
            }
        }
    }
}