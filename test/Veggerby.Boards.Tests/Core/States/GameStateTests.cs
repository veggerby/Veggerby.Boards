using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States
{
    public class GameStateTests
    {
        public Game Game { get; }
        public GameStateTests()
        {
            Game = new TestGameBuilder().Compile();
        }

        public class New : GameStateTests
        {
            [Fact]
            public void Should_create_new_gamestate_with_empty()
            {
                // arrange

                // act
                var actual = GameState.New(Game, null, null);

                // assert
                actual.ShouldNotBeNull();
                actual.IsInitialState.ShouldBeTrue();
                actual.Game.ShouldBeSameAs(Game);
                actual.ChildStates.ShouldBeEmpty();
            }

            [Fact]
            public void Should_create_new_gamestate_child_states()
            {
                // arrange
                var piece = Game.GetPiece("piece-1");
                var tile = Game.GetTile("tile-1");
                var state1 = new PieceState(piece, tile);

                // act
                var actual = GameState.New(Game, null, null);

                // assert
                actual.ShouldNotBeNull();
                actual.IsInitialState.ShouldBeTrue();
                actual.Game.ShouldBeSameAs(Game);
                actual.ChildStates.ShouldBeEmpty();
            }

            [Fact]
            public void Should_throw_with_null_game()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => GameState.New(null, null, null));

                // assert
                actual.ParamName.ShouldBe("game");
            }
        }

        public class GetState : GameStateTests
        {
            [Fact]
            public void Should_return_artifact_getstate()
            {
                // arrange
                var piece = Game.GetPiece("piece-1");
                var tile = Game.GetTile("tile-1");
                var expected = new PieceState(piece, tile);
                var gameState = GameState.New(Game, new [] { expected }, null);

                // act
                var actual = gameState.GetState(piece);

                // assert
                actual.ShouldNotBeNull();
                actual.ShouldBeOfType<PieceState>();
                actual.Artifact.ShouldBe(piece);
            }

            [Fact]
            public void Should_throw_with_null_game()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => GameState.New(null, null, null));

                // assert
                actual.ParamName.ShouldBe("game");
            }
        }

        public class IsInitialState : GameStateTests
        {
            [Fact]
            public void Should_return_initial_state()
            {
                // arrange
                var piece = Game.GetPiece("piece-1");
                var tile = Game.GetTile("tile-1");
                var expected = new PieceState(piece, tile);
                var gameState = GameState.New(Game, new [] { expected }, null);

                // act
                var actual = gameState.IsInitialState;

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_return_initial_state()
            {
                // arrange
                var piece = Game.GetPiece("piece-1");
                var tile = Game.GetTile("tile-1");
                var expected = new PieceState(piece, tile);
                var gameState = GameState.New(Game, null, null);
                gameState = GameState.New(Game, new [] { expected }, gameState);

                // act
                var actual = gameState.IsInitialState;

                // assert
                actual.ShouldBeFalse();
            }
        }
    }
}