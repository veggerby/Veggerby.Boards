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
                var dice = new Dice("dice");

                // act
                var actual = new NullDiceState(dice);

                // assert
                actual.Artifact.ShouldBe(dice);
            }

            [Fact]
            public void Should_throw_when_null_dice()
            {
                // assert

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new NullDiceState(null));

                // assert
                actual.ParamName.ShouldBe("artifact");
            }
        }

        public class _Equals
        {
            [Fact]
            public void Should_equal_self()
            {
                // arrange
                var dice = new Dice("dice");
                var state = new NullDiceState(dice);

                // act
                var actual = state.Equals(state);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var dice = new Dice("dice");
                var state = new NullDiceState(dice);

                // act
                var actual = state.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_other_null_state_same_artifact()
            {
                // arrange
                var dice = new Dice("dice");
                var state1 = new NullDiceState(dice);
                var state2 = new NullDiceState(dice);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_different_artifacts()
            {
                // arrange
                var dice1 = new Dice("dice-1");
                var dice2 = new Dice("dice-2");
                var state1 = new NullDiceState(dice1);
                var state2 = new NullDiceState(dice2);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_artifact_state_type()
            {
                // arrange
                var dice = new Dice("dice");
                var piece = new Piece("piece", null, null);
                var tile = new Tile("tile");
                var state1 = new NullDiceState(dice);
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
                var dice = new Dice("dice");
                var state = new NullDiceState(dice);

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
                var dice = new Dice("dice");
                var expected = (typeof(NullDiceState).GetHashCode() * 397) ^ dice.GetHashCode();
                var state = new NullDiceState(dice);

                // act
                var actual = state.GetHashCode();

                // assert
                actual.ShouldBe(expected);
            }
        }
    }
}