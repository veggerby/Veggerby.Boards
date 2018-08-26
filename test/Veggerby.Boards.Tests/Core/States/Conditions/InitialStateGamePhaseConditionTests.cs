using Shouldly;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States.Conditions
{
    public class InitialStateGamePhaseConditionTests
    {
        public class Evaluate
        {
            [Fact]
            public void Should_evaluate_true_on_initial_state()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null, null);

                var condition = new InitialStateGamePhaseCondition();

                // act
                var actual = condition.Evaluate(state);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_evaluate_false_on_non_initial_state()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null, null);
                state = GameState.New(game, null, state);

                var condition = new InitialStateGamePhaseCondition();

                // act
                var actual = condition.Evaluate(state);

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}