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

        public class _Equals
        {
            [Fact]
            public void Should_equal_self()
            {
                // arrange
                var dice = new RegularDice("dice");
                var state = new DiceState<int>(dice, 5);

                // act
                var actual = state.Equals(state);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var dice = new RegularDice("dice");
                var state = new DiceState<int>(dice, 5);

                // act
                var actual = state.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_other_dice_state_same_artifact_same_value()
            {
                // arrange
                var dice = new RegularDice("dice");
                var state1 = new DiceState<int>(dice, 5);
                var state2 = new DiceState<int>(dice, 5);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_equal_other_dice_state_same_artifact_different_value()
            {
                // arrange
                var dice = new RegularDice("dice");
                var state1 = new DiceState<int>(dice, 5);
                var state2 = new DiceState<int>(dice, 3);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_artifacts()
            {
                // arrange
                var dice1 = new RegularDice("dice-1");
                var dice2 = new RegularDice("dice-2");
                var state1 = new DiceState<int>(dice1, 5);
                var state2 = new DiceState<int>(dice2, 5);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_artifact_state_type()
            {
                // arrange
                var dice = new RegularDice("dice");
                var piece = new Piece("piece", null, null);
                var tile = new Tile("tile");
                var state1 = new DiceState<int>(dice, 5);
                var state2 = new PieceState(piece, tile);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_another_type()
            {
                // arrange
                var dice = new RegularDice("dice");
                var state = new DiceState<int>(dice, 5);

                // act
                var actual = state.Equals("a string");

                // assert
                actual.ShouldBeFalse();
            }
        }

        public class _GetHashCode
        {
            [Fact]
            public void Should_equal_self()
            {
                // arrange
                var dice = new RegularDice("dice");
                var expected = ((typeof(DiceState<int>).GetHashCode() * 397) ^ dice.GetHashCode()) * 397 ^ 3.GetHashCode();
                var state = new DiceState<int>(dice, 3);

                // act
                var actual = state.GetHashCode();

                // assert
                actual.ShouldBe(expected);
            }
        }
    }
}