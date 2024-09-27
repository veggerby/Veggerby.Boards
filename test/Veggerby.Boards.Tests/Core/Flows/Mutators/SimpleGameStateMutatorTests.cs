using System;

using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class SimpleGameStateMutatorTests
{
    public class Create
    {
        [Fact]
        public void Should_initialize_mutator()
        {
            // arrange

            // act
            var actual = new SimpleGameStateMutator<MovePieceGameEvent>(e => new PieceState(e.Piece, e.To));

            // assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void Should_throw_null_state_func()
        {
            // arrange

            // act
            var actual = () => new SimpleGameStateMutator<NullGameEvent>(null);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("stateFunc");
        }
    }

    public class MutateState
    {
        [Fact]
        public void Should_mutate_simple_state()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var piece = game.GetPiece("piece-1");
            var state = initialState.GetState<PieceState>(piece);
            var toTile = game.GetTile("tile-2");

            var path = new TilePath([new TileRelation(state.CurrentTile, toTile, Direction.Clockwise)]);
            var @event = new MovePieceGameEvent(piece, path);

            var mutator = new SimpleGameStateMutator<MovePieceGameEvent>(e => new PieceState(e.Piece, e.To));

            // act
            var actual = mutator.MutateState(engine.Engine, initialState, @event);

            // assert
            actual.Should().NotBe(initialState);
            actual.IsInitialState.Should().BeFalse();
            var pieceState = actual.GetState<PieceState>(piece);
            pieceState.CurrentTile.Should().Be(toTile);
        }

        [Fact]
        public void Should_return_original_when_null_artifactstate()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var @event = new NullGameEvent();

            var mutator = new SimpleGameStateMutator<NullGameEvent>(e => null);

            // act
            var actual = mutator.MutateState(engine.Engine, initialState, @event);

            // assert
            actual.Should().Be(initialState);
        }
    }
}