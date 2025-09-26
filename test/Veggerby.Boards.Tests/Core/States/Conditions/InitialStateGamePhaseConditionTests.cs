using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.States.Conditions;

public class InitialStateGamePhaseConditionTests
{
    public class Evaluate
    {
        [Fact]
        public void Should_evaluate_true_on_initial_state()
        {
            // arrange
            var state = GameState.New(null);

            var condition = new InitialGameStateCondition();

            // act
            var actual = condition.Evaluate(state);

            // assert
            actual.Should().Be(ConditionResponse.Valid);
        }

        [Fact]
        public void Should_evaluate_false_on_non_initial_state()
        {
            // arrange
            var state = GameState.New(null).Next(null);

            var condition = new InitialGameStateCondition();

            // act
            var actual = condition.Evaluate(state);

            // assert
            actual.Should().Be(ConditionResponse.Invalid);
        }
    }
}