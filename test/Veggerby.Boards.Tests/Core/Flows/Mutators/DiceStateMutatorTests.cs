using System;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Utils;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators
{
    public class DiceStateMutatorTests
    {
        public class MutateState
        {
            [Fact]
            public void Should_update_state()
            {
                // arrange
                var engine = new TestGameBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.State;
                var mutator = new DiceStateMutator<int>();
                var dice = game.GetArtifact<Dice>("dice");
                var @event = new RollDiceGameEvent<int>(new DiceState<int>(dice, 4));

                // act
                var actual = mutator.MutateState(engine.Engine, initialState, @event);

                // assert
                actual.ShouldNotBeSameAs(initialState);
                actual.IsInitialState.ShouldBeFalse();
                var diceState = actual.GetState<DiceState<int>>(dice);
                diceState.CurrentValue.ShouldBe(4);
            }
        }
    }
}