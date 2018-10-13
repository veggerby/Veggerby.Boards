using System;
using Shouldly;
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
    public class NullStateMutatorTests
    {
        public class MutateState
        {
            [Fact]
            public void Should_return_same_state()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.State;
                var mutator = new NullStateMutator<NullGameEvent>();
                var @event = new NullGameEvent();

                // act
                var actual = mutator.MutateState(initialState, @event);

                // assert
                actual.ShouldBeSameAs(initialState);
            }
        }
    }
}