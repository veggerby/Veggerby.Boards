using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators
{
    public class SimpleGameStateMutatorTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initialize_mutator()
            {
                // arrange

                // act
                var actual = new SimpleGameStateMutator<MovePieceGameEvent>(e => new PieceState(e.Piece, e.To));

                // assert
                actual.ShouldNotBeNull();
            }

            [Fact]
            public void Should_throw_null_state_func()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new SimpleGameStateMutator<NullGameEvent>(null));

                // assert
                actual.ParamName.ShouldBe("stateFunc");
            }
        }

        public class MutateState
        {
            [Fact]
            public void Should_mutate_simple_state()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var initialState = new TestInitialGameStateBuilder().Compile(game);
                var piece = game.GetPiece("piece-1");
                var state = initialState.GetState<PieceState>(piece);
                var toTile = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, state.CurrentTile, toTile);

                var mutator = new SimpleGameStateMutator<MovePieceGameEvent>(e => new PieceState(e.Piece, e.To));

                // act
                var actual = mutator.MutateState(initialState, @event);

                // assert
                actual.ShouldNotBeSameAs(initialState);
                actual.IsInitialState.ShouldBeFalse();
                var pieceState = actual.GetState<PieceState>(piece);
                pieceState.CurrentTile.ShouldBe(toTile);
            }

            [Fact]
            public void Should_return_original_when_null_artifactstate()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var initialState = new TestInitialGameStateBuilder().Compile(game);
                var @event = new NullGameEvent();

                var mutator = new SimpleGameStateMutator<NullGameEvent>(e => null);

                // act
                var actual = mutator.MutateState(initialState, @event);

                // assert
                actual.ShouldBeSameAs(initialState);
            }
        }
    }
}