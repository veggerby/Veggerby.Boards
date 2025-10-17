using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.States;

public class DiceStateTests
{
    public class Create
    {
        [Fact]
        public void Should_create_dice_state()
        {
            // assert
            var dice = new Dice("dice"); dice.Should().NotBeNull();

            // act
            var actual = new DiceState<int>(dice, 5);

            // assert
            actual.Artifact.Should().Be(dice);
            actual.CurrentValue.Should().Be(5);
        }

        [Fact]
        public void Should_throw_when_null_dice()
        {
            // assert

            // act
            var actual = () => new DiceState<int>(null!, 5);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("artifact");
        }

        [Fact]
        public void Should_throw_when_null_value()
        {
            // assert
            var dice = new Dice("dice");

            // act
            var actual = () => new DiceState<string>(dice, null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("currentValue");
        }
    }

    public class _Equals
    {
        [Fact]
        public void Should_equal_self()
        {
            // arrange
            var dice = new Dice("dice");
            var state = new DiceState<int>(dice, 5);

            // act
            var actual = state.Equals(state);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var dice = new Dice("dice");
            var state = new DiceState<int>(dice, 5);

            // act
            var actual = state.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_other_dice_state_same_artifact_same_value()
        {
            // arrange
            var dice = new Dice("dice");
            var state1 = new DiceState<int>(dice, 5);
            var state2 = new DiceState<int>(dice, 5);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_equal_other_dice_state_same_artifact_different_value()
        {
            // arrange
            var dice = new Dice("dice");
            var state1 = new DiceState<int>(dice, 5);
            var state2 = new DiceState<int>(dice, 3);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_different_artifacts()
        {
            // arrange
            var dice1 = new Dice("dice-1");
            var dice2 = new Dice("dice-2");
            var state1 = new DiceState<int>(dice1, 5);
            var state2 = new DiceState<int>(dice2, 5);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_different_artifact_state_type()
        {
            // arrange
            var dice = new Dice("dice");
            var owner = new Player("p");
            var piece = new Piece("piece", owner, System.Array.Empty<Veggerby.Boards.Artifacts.Patterns.IPattern>());
            var tile = new Tile("tile");
            var state1 = new DiceState<int>(dice, 5);
            var state2 = new PieceState(piece, tile);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_another_type()
        {
            // arrange
            var dice = new Dice("dice");
            var state = new DiceState<int>(dice, 5);

            // act
            var actual = state.Equals("a string");

            // assert
            actual.Should().BeFalse();
        }
    }
}