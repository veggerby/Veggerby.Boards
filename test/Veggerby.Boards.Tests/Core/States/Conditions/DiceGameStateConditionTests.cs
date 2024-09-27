using System;
using System.Linq;

using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Tests.Core.States.Conditions
{
    public class DiceGameStateConditionTests
    {
        public class Create
        {
            [Fact]
            public void Should_create_with_dice()
            {
                // arrange
                var dice = new Dice("dice");

                // act
                var actual = new DiceGameStateCondition<int>([dice], CompositeMode.All);

                // assert
                actual.Dice.Should().Equal([dice]);
            }

            [Fact]
            public void Should_create_distinct_list_with_dice()
            {
                // arrange
                var dice = new Dice("dice");

                // act
                var actual = new DiceGameStateCondition<int>([dice, dice], CompositeMode.All);

                // assert
                actual.Dice.Should().Equal([dice]);
            }

            [Fact]
            public void Should_throw_when_null_dice()
            {
                // arrange
                var dice = new Dice("dice");

                // act
                var actual = () => new DiceGameStateCondition<int>(null, CompositeMode.All);

                // assert
                actual.Should().Throw<ArgumentNullException>().WithParameterName("dice");
            }

            [Fact]
            public void Should_throw_when_empty_dice()
            {
                // arrange
                // act
                var actual = () => new DiceGameStateCondition<int>(Enumerable.Empty<Dice>(), CompositeMode.All);

                // assert
                actual.Should().Throw<ArgumentException>().WithParameterName("dice");
            }
        }

        public class Evaluate
        {
            [Fact]
            public void Should_evaluate_true_when_single_dice_has_state()
            {
                // arrange
                var dice = new Dice("dice");
                var diceState = new DiceState<int>(dice, 5);
                var gameState = GameState.New([diceState]);
                var condition = new DiceGameStateCondition<int>([dice], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Valid);
            }

            [Fact]
            public void Should_evaluate_false_when_single_dice_does_not_have_state()
            {
                // arrange
                var dice = new Dice("dice");
                var gameState = GameState.New(null);
                var condition = new DiceGameStateCondition<int>([dice], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Invalid);
            }

            [Fact]
            public void Should_evaluate_false_when_single_dice_does_not_have_state_but_other_dice_has()
            {
                // arrange
                var dice1 = new Dice("dice1");
                var dice2 = new Dice("dice2");
                var diceState = new DiceState<int>(dice1, 5);
                var gameState = GameState.New([diceState]);
                var condition = new DiceGameStateCondition<int>([dice2], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Invalid);
            }

            [Fact]
            public void Should_evaluate_false_when_single_dice_has_null_state()
            {
                // arrange
                var dice = new Dice("dice");
                var diceState = new NullDiceState(dice);
                var gameState = GameState.New([diceState]);
                var condition = new DiceGameStateCondition<int>([dice], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Invalid);
            }

            [Fact]
            public void Should_evaluate_true_with_multiple_dice_with_state()
            {
                // arrange
                var dice1 = new Dice("dice1");
                var dice2 = new Dice("dice2");
                var diceState1 = new DiceState<int>(dice1, 2);
                var diceState2 = new DiceState<int>(dice2, 3);
                var gameState = GameState.New([diceState1, diceState2]);
                var condition = new DiceGameStateCondition<int>([dice1, dice2], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Valid);
            }

            [Fact]
            public void Should_evaluate_false_with_multiple_dice_only_one_state()
            {
                // arrange
                var dice1 = new Dice("dice1");
                var dice2 = new Dice("dice2");
                var diceState1 = new DiceState<int>(dice1, 2);
                var gameState = GameState.New([diceState1]);
                var condition = new DiceGameStateCondition<int>([dice1, dice2], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Invalid);
            }


            [Fact]
            public void Should_evaluate_false_with_multiple_dice_null_state()
            {
                // arrange
                var dice1 = new Dice("dice1");
                var dice2 = new Dice("dice2");
                var diceState1 = new NullDiceState(dice1);
                var diceState2 = new NullDiceState(dice2);
                var gameState = GameState.New([diceState1, diceState2]);
                var condition = new DiceGameStateCondition<int>([dice1, dice2], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Invalid);
            }

            [Fact]
            public void Should_evaluate_false_with_multiple_dice_no_state()
            {
                // arrange
                var dice1 = new Dice("dice1");
                var dice2 = new Dice("dice2");
                var gameState = GameState.New(null);
                var condition = new DiceGameStateCondition<int>([dice1, dice2], CompositeMode.All);

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.Should().Be(ConditionResponse.Invalid);
            }
        }
    }
}