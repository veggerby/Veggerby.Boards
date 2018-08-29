using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
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

                // act
                var actual = new RollDiceGameEvent<int>(dice, 4);

                // assert
                actual.Dice.ShouldBe(dice);
                actual.NewDiceValue.ShouldBe(4);
            }

            [Fact]
            public void Should_throw_with_null_dice()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new RollDiceGameEvent<int>(null, 3));

                // assert
                actual.ParamName.ShouldBe("dice");
            }

            [Fact]
            public void Should_throw_with_null_value()
            {
                // arrange
                var dice = new Dice<string>("dice");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new RollDiceGameEvent<string>(dice, null));

                // assert
                actual.ParamName.ShouldBe("newDiceValue");
            }

            [Fact]
            public void Should_throw_with_null_to_tile()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new DirectionPattern(Direction.Clockwise, true) });
                var from = new Tile("tile-1");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new MovePieceGameEvent(piece, from, null));

                // assert
                actual.ParamName.ShouldBe("to");
            }
        }
    }
}