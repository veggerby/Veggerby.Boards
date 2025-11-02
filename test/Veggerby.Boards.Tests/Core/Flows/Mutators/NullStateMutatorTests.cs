using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class NullStateMutatorTests
{
    public class MutateState
    {
        [Fact]
        public void Should_return_same_state()
        {
            // arrange

            // act

            // assert

            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var mutator = new NullStateMutator<NullGameEvent>();
            var @event = new NullGameEvent();

            // act
            var actual = mutator.MutateState(engine.Engine, initialState, @event);

            // assert
            actual.Should().Be(initialState);
        }
    }
}
