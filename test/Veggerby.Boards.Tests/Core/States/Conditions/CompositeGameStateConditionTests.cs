using System;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States.Conditions
{
    public class CompositeGameStateConditionTests
    {
        public class ctor
        {
            // main ctor tested via ConditionExtensions

            [Fact]
            public void Should_throw_with_null_child_condition_and()
            {
                // arrange
                IGameStateCondition condition1 = null;
                IGameStateCondition condition2 = null;

                // act
                var actual = Should.Throw<ArgumentException>(() => condition1.And(condition2));

                // assert
                actual.ParamName.ShouldBe("childConditions");
            }

            [Fact]
            public void Should_throw_with_null_child_condition_or()
            {
                // arrange
                IGameStateCondition condition1 = null;
                IGameStateCondition condition2 = null;

                // act
                var actual = Should.Throw<ArgumentException>(() => condition1.Or(condition2));

                // assert
                actual.ParamName.ShouldBe("childConditions");
            }        }

        public class Evaluate
        {
            private readonly Game _game;
            private readonly GameState _state;

            public Evaluate()
            {
                var engine = new TestGameEngineBuilder().Compile();
                _game = engine.Game;
                _state = GameState.New(_game, null);
            }

            [Fact]
            public void Should_evaluate_true_when_all_are_true_when_mode_all()
            {
                // arrange
                var condition = new NullGameStateCondition(true)
                    .And(new NullGameStateCondition(true));

                // act
                var actual = condition.Evaluate(_state);

                // assert
                actual.ShouldBe(ConditionResponse.Valid);
            }

            [Fact]
            public void Should_evaluate_false_when_not_all_are_true_when_mode_all()
            {
                // arrange
                var condition = new NullGameStateCondition(true)
                    .And(new NullGameStateCondition(false));

                // act
                var actual = condition.Evaluate(_state);

                // assert
                actual.ShouldBe(ConditionResponse.Invalid);
            }

            [Fact]
            public void Should_evaluate_false_when_none_are_true_when_mode_all()
            {
                // arrange
                var condition = new NullGameStateCondition(false)
                    .And(new NullGameStateCondition(false));

                // act
                var actual = condition.Evaluate(_state);

                // assert
                actual.ShouldBe(ConditionResponse.Invalid);
            }

            [Fact]
            public void Should_evaluate_true_when_all_are_true_when_mode_any()
            {
                // arrange
                var condition = new NullGameStateCondition(true)
                    .Or(new NullGameStateCondition(true));

                // act
                var actual = condition.Evaluate(_state);

                // assert
                actual.ShouldBe(ConditionResponse.Valid);
            }

            [Fact]
            public void Should_evaluate_true_when_not_all_are_true_when_mode_any()
            {
                // arrange
                var condition = new NullGameStateCondition(true)
                    .Or(new NullGameStateCondition(false));

                // act
                var actual = condition.Evaluate(_state);

                // assert
                actual.ShouldBe(ConditionResponse.Valid);
            }

            [Fact]
            public void Should_evaluate_false_when_none_are_true_when_mode_any()
            {
                // arrange
                var condition = new NullGameStateCondition(false)
                    .Or(new NullGameStateCondition(false));

                // act
                var actual = condition.Evaluate(_state);

                // assert
                actual.ShouldBe(ConditionResponse.Invalid);
            }
        }
    }
}