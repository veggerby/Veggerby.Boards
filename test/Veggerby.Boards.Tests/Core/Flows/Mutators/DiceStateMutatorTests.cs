using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class DiceStateMutatorTests
{
    public class MutateState
    {
        [Fact]
        public void Should_update_state()
        {
            // arrange

            // act

            // assert

            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var mutator = new DiceStateMutator<int>();
            var dice = game.GetArtifact<Dice>("dice");
            dice.Should().NotBeNull();
            var @event = new RollDiceGameEvent<int>(new DiceState<int>(dice!, 4));

            // act
            var actual = mutator.MutateState(engine.Engine, initialState, @event);

            // assert
            actual.Should().NotBe(initialState);
            actual.IsInitialState.Should().BeFalse();
            var diceState = actual.GetState<DiceState<int>>(dice!);
            diceState.Should().NotBeNull();
            diceState!.CurrentValue.Should().Be(4);
        }
    }
}
