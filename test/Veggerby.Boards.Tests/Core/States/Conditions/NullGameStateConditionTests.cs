using Shouldly;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States.Conditions
{
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
                var game = new TestGameEngineBuilder().Compile().Game;
                var state = GameState.New(game, null);
                var condition = new NullGameStateCondition(defaultValue);

                // act
                var actual = condition.Evaluate(state);

                // assert
                actual.ShouldBe(defaultValue);
            }
        }
    }
}