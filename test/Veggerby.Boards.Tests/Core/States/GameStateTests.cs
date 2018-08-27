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
                var actual = GameState.New(Game, null);

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
                var actual = GameState.New(Game, null);

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
                var actual = Should.Throw<ArgumentNullException>(() => GameState.New(null, null));

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
                var gameState = GameState.New(Game, new [] { expected });

                // act
                var actual = gameState.GetState<PieceState>(piece);

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
                var actual = Should.Throw<ArgumentNullException>(() => GameState.New(null, null));

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
                var gameState = GameState.New(Game, new [] { expected });

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
                var gameState = GameState.New(Game, null)
                    .Next(new [] { expected });

                // act
                var actual = gameState.IsInitialState;

                // assert
                actual.ShouldBeFalse();
            }
        }

        public class Next : GameStateTests
        {
            [Fact]
            public void Should_return_next_state()
            {
                // arrange
                var piece = Game.GetPiece("piece-1");
                var tile1 = Game.GetTile("tile-1");
                var tile2 = Game.GetTile("tile-2");
                var dice = new RegularDice("dice");
                var pieceState1 = new PieceState(piece, tile1);
                var pieceState2 = new PieceState(piece, tile2);
                var diceState = new DiceState<int>(dice, 5);
                var gameState = GameState.New(Game, new IArtifactState[] { diceState, pieceState1 });

                // act
                var actual = gameState.Next(new [] { pieceState2 });

                // assert
                actual.ShouldNotBeNull();
                actual.Game.ShouldBe(Game);
                actual.IsInitialState.ShouldBeFalse();
                actual.ChildStates.ShouldBe(new IArtifactState[] { diceState, pieceState2 });
            }
        }
    }
}