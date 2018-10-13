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
    public class PieceStateMutatorTests
    {
        public class MutateState
        {
            [Fact]
            public void Should_update_state()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.State;
                var mutator = new MovePieceStateMutator();
                var piece = game.GetPiece("piece-1");
                var state = initialState.GetState<PieceState>(piece);
                var toTile = game.GetTile("tile-2");
                var @event = new MovePieceGameEvent(piece, state.CurrentTile, toTile);

                // act
                var actual = mutator.MutateState(initialState, @event);

                // assert
                actual.ShouldNotBeSameAs(initialState);
                actual.IsInitialState.ShouldBeFalse();
                var pieceState = actual.GetState<PieceState>(piece);
                pieceState.CurrentTile.ShouldBe(toTile);
            }

            [Fact]
            public void Should_throw_with_invalid_from_tile()
            {
                // arrange
                var engine = new TestGameEngineBuilder().Compile();
                var game = engine.Game;
                var initialState = engine.State;
                var mutator = new MovePieceStateMutator();
                var piece = game.GetPiece("piece-1");
                var state = initialState.GetState<PieceState>(piece);

                var fromTile = game.GetTile("tile-2");
                var toTile = game.GetTile("tile-1");
                var @event = new MovePieceGameEvent(piece, fromTile, toTile);

                // act
                var actual = Should.Throw<BoardException>(() => mutator.MutateState(initialState, @event));

                // assert
                actual.Message.ShouldBe("Invalid from tile on move piece event");
            }
        }
    }
}