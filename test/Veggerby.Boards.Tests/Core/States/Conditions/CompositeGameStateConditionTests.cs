using System;

using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.States.Conditions;

public class CompositeGameStateConditionTests
{
    public class Create
    {
        // main ctor tested via ConditionExtensions

        [Fact]
        public void Should_throw_with_null_child_condition_and()
        {
            // arrange
            IGameStateCondition? condition1 = null;
            IGameStateCondition? condition2 = null;

            // act
            var actual = () => condition1!.And(condition2!);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("childConditions");
        }

        [Fact]
        public void Should_throw_with_null_child_condition_or()
        {
            // arrange
            IGameStateCondition? condition1 = null;
            IGameStateCondition? condition2 = null;

            // act
            var actual = () => condition1!.Or(condition2!);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("childConditions");
        }
    }

    public class Evaluate
    {
        private readonly GameState _state;

        public Evaluate()
        {
            _state = GameState.New(System.Array.Empty<IArtifactState>());
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
            actual.Should().Be(ConditionResponse.Valid);
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
            actual.Should().Be(ConditionResponse.Invalid);
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
            actual.Should().Be(ConditionResponse.Invalid);
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
            actual.Should().Be(ConditionResponse.Valid);
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
            actual.Should().Be(ConditionResponse.Valid);
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
            actual.Should().Be(ConditionResponse.Invalid);
        }
    }
}