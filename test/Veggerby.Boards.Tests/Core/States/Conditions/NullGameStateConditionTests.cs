using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.States.Conditions;

public class NullGameStateConditionTests
{
    public class Evaluate
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_evaluate_true_on_initial_state(bool defaultValue)
        {
            // arrange
            var state = GameState.New(null);
            var condition = new NullGameStateCondition(defaultValue);

            // act
            var actual = condition.Evaluate(state);

            // assert
            actual.Equals(ConditionResponse.Valid).Should().Be(defaultValue);
        }
    }
}