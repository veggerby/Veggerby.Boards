using Veggerby.Boards;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class PieceStateMutatorTests
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
            var mutator = new MovePieceStateMutator();
            var piece = game.GetPiece("piece-1");
            var state = initialState.GetState<PieceState>(piece);
            var toTile = game.GetTile("tile-2");
            var path = new TilePath([new TileRelation(state.CurrentTile, toTile, Direction.Clockwise)]);
            var @event = new MovePieceGameEvent(piece, path);

            // act
            var actual = mutator.MutateState(engine.Engine, initialState, @event);

            // assert
            actual.Should().NotBe(initialState);
            actual.IsInitialState.Should().BeFalse();
            var pieceState = actual.GetState<PieceState>(piece);
            pieceState.CurrentTile.Should().Be(toTile);
        }

        [Fact]
        public void Should_throw_with_invalid_from_tile()
        {
            // arrange
            var engine = new TestGameBuilder().Compile();
            var game = engine.Game;
            var initialState = engine.State;
            var mutator = new MovePieceStateMutator();
            var piece = game.GetPiece("piece-1");
            var state = initialState.GetState<PieceState>(piece);

            var fromTile = game.GetTile("tile-2");
            var toTile = game.GetTile("tile-1");

            var path = new TilePath([new TileRelation(fromTile, toTile, Direction.CounterClockwise)]);
            var @event = new MovePieceGameEvent(piece, path);

            // act
            var actual = () => mutator.MutateState(engine.Engine, initialState, @event);

            // assert
            actual.Should().Throw<BoardException>().WithMessage("*Invalid from tile on move piece event*");
        }
    }
}