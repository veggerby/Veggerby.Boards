using Shouldly;
using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States
{
    public class NullDiceStateTests
    {
        public class ctor
        {
            [Fact]
            public void Should_create_dice_state()
            {
                // assert
                var dice = new Dice<int>("dice");

                // act
                var actual = new NullDiceState<int>(dice);

                // assert
                actual.Artifact.ShouldBe(dice);
            }

            [Fact]
            public void Should_throw_when_null_dice()
            {
                // assert

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new NullDiceState<int>(null));

                // assert
                actual.ParamName.ShouldBe("artifact");
            }
        }
    }
}