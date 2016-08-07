using System;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Events
{
    public class GameEventHandlerTests
    {
        public class ctor
        {
            [Fact]
            public void Should_throw_if_eventhandlers_are_null()
            {
                // arrange

                // act
                var actual = Assert.Throws<ArgumentNullException>(() => new GameEventHandler<Artifact>(null, null));

                // assert
                Assert.Equal("onAfterEvent", actual.ParamName);
            }
        }

        public class OnBeforeEvent
        {
            [Fact]
            public void Should_handle_on_before_event()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new AnyPattern() });
                var tile1 = new Tile("tile-1");
                var tile2 = new Tile("tile-2");
                var currentState = new PieceState(piece, tile1);

                var handler = new GameEventHandler<Piece>(
                    (state, @event) => new PieceState(piece, tile2),
                    null
                );
                
                // act
                var actual = handler.OnBeforeEvent(currentState, new NullEvent());
                
                // assert
                Assert.IsType<PieceState>(actual);
                Assert.Equal(piece, actual.Artifact);
                Assert.Equal(tile2, ((PieceState)actual).CurrentTile);
            }
            
            [Fact]
            public void Should_return_same_state_when_no_mutator()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new AnyPattern() });
                var tile1 = new Tile("tile-1");
                var tile2 = new Tile("tile-2");
                var currentState = new PieceState(piece, tile1);

                var handler = new GameEventHandler<Piece>(
                    null,
                    (state, @event) => null // to avoid exception
                );
                
                // act
                var actual = handler.OnBeforeEvent(currentState, new NullEvent());
                
                // assert
                Assert.Equal(currentState, actual);
            }
            
            [Fact]
            public void Should_return_same_state_when_mutator_returns_null()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new AnyPattern() });
                var tile1 = new Tile("tile-1");
                var tile2 = new Tile("tile-2");
                var currentState = new PieceState(piece, tile1);

                var handler = new GameEventHandler<Piece>(
                    (state, @event) => null,
                    null
                );
                
                // act
                var actual = handler.OnBeforeEvent(currentState, new NullEvent());
                
                // assert
                Assert.Equal(currentState, actual);
            }

            [Fact]
            public void Should_throw_exception_when_event_is_null()
            {
                // arrange
                var piece1 = new Piece("piece-1", null, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", null, new [] { new AnyPattern() });
                var tile1 = new Tile("tile-1");
                var tile2 = new Tile("tile-2");
                var currentState = new PieceState(piece1, tile1);

                var handler = new GameEventHandler<Piece>(
                    (state, @event) => null,
                    null
                );
                
                // act
                var actual = Assert.Throws<ArgumentNullException>(() => handler.OnBeforeEvent(currentState, null));
                
                // assert
                Assert.Equal("event", actual.ParamName);
            }

            [Fact]
            public void Should_throw_exception_when_new_state_is_different_artifact()
            {
                // arrange
                var piece1 = new Piece("piece-1", null, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", null, new [] { new AnyPattern() });
                var tile1 = new Tile("tile-1");
                var tile2 = new Tile("tile-2");
                var currentState = new PieceState(piece1, tile1);

                var handler = new GameEventHandler<Piece>(
                    (state, @event) => new PieceState(piece2, tile2),
                    null
                );
                
                // act
                var actual = Assert.Throws<BoardException>(() => handler.OnBeforeEvent(currentState, new NullEvent()));
                
                // assert
                Assert.Equal("Invalid artifact on state change", actual.Message);
            }
        }
    }
}