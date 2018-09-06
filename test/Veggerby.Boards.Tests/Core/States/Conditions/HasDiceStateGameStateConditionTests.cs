using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States.Conditions
{
    public class HasDiceStateGameStateConditionTests
    {
        public class ctor
        {
            [Fact]
            public void Should_create_with_dice()
            {
                // arrange
                var dice = new RegularDice("dice");

                // act
                var actual = new DiceGameStateCondition<RegularDice, int>(new[] { dice });

                // assert
                actual.Dice.ShouldBe(new[] { dice });
            }

            [Fact]
            public void Should_create_distinct_list_with_dice()
            {
                // arrange
                var dice = new RegularDice("dice");

                // act
                var actual = new DiceGameStateCondition<RegularDice, int>(new[] { dice, dice });

                // assert
                actual.Dice.ShouldBe(new[] { dice });
            }

            [Fact]
            public void Should_throw_when_null_dice()
            {
                // arrange
                var dice = new RegularDice("dice");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new DiceGameStateCondition<RegularDice, int>(null));

                // assert
                actual.ParamName.ShouldBe("dice");
            }

            [Fact]
            public void Should_throw_when_empty_dice()
            {
                // arrange
                // act
                var actual = Should.Throw<ArgumentException>(() => new DiceGameStateCondition<RegularDice, int>(Enumerable.Empty<RegularDice>()));

                // assert
                actual.ParamName.ShouldBe("dice");
            }
        }

        public class Evaluate
        {
            private readonly Game _game;

            public Evaluate()
            {
                _game = new TestGameEngineBuilder().Compile().Game;
            }

            [Fact]
            public void Should_evaluate_true_when_single_dice_has_state()
            {
                // arrange
                var dice = new RegularDice("dice");
                var diceState = new DiceState<int>(dice, 5);
                var gameState = GameState.New(_game, new [] { diceState });
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_evaluate_false_when_single_dice_does_not_have_state()
            {
                // arrange
                var dice = new RegularDice("dice");
                var gameState = GameState.New(_game, null);
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_evaluate_false_when_single_dice_does_not_have_state_but_other_dice_has()
            {
                // arrange
                var dice1 = new RegularDice("dice1");
                var dice2 = new RegularDice("dice2");
                var diceState = new DiceState<int>(dice1, 5);
                var gameState = GameState.New(_game, new [] { diceState });
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice2 });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_evaluate_false_when_single_dice_has_null_state()
            {
                // arrange
                var dice = new RegularDice("dice");
                var diceState = new NullDiceState<int>(dice);
                var gameState = GameState.New(_game, new [] { diceState });
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_evaluate_true_with_multiple_dice_with_state()
            {
                // arrange
                var dice1 = new RegularDice("dice1");
                var dice2 = new RegularDice("dice2");
                var diceState1 = new DiceState<int>(dice1, 2);
                var diceState2 = new DiceState<int>(dice2, 3);
                var gameState = GameState.New(_game, new [] { diceState1, diceState2 });
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice1, dice2 });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_evaluate_false_with_multiple_dice_only_one_state()
            {
                // arrange
                var dice1 = new RegularDice("dice1");
                var dice2 = new RegularDice("dice2");
                var diceState1 = new DiceState<int>(dice1, 2);
                var gameState = GameState.New(_game, new [] { diceState1 });
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice1, dice2 });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeFalse();
            }


            [Fact]
            public void Should_evaluate_false_with_multiple_dice_null_state()
            {
                // arrange
                var dice1 = new RegularDice("dice1");
                var dice2 = new RegularDice("dice2");
                var diceState1 = new NullDiceState<int>(dice1);
                var diceState2 = new NullDiceState<int>(dice2);
                var gameState = GameState.New(_game, new [] { diceState1, diceState2 });
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice1, dice2 });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_evaluate_false_with_multiple_dice_no_state()
            {
                // arrange
                var dice1 = new RegularDice("dice1");
                var dice2 = new RegularDice("dice2");
                var gameState = GameState.New(_game, null);
                var condition = new DiceGameStateCondition<RegularDice, int>(new[] { dice1, dice2 });

                // act
                var actual = condition.Evaluate(gameState);

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}