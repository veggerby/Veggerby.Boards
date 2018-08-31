using System;
using System.Linq;
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

        public class CompareTo : GameStateTests
        {
            [Fact]
            public void Should_return_no_changes_when_equals()
            {
                // arrange
                var piece1 = Game.GetPiece("piece-1");
                var piece2 = Game.GetPiece("piece-2");
                var tile1 = Game.GetTile("tile-1");
                var tile2 = Game.GetTile("tile-2");
                var dice = new RegularDice("dice");
                var pieceState1 = new PieceState(piece1, tile1);
                var pieceState2 = new PieceState(piece2, tile2);
                var diceState = new DiceState<int>(dice, 5);
                var gameState = GameState.New(Game, new IArtifactState[] { diceState, pieceState1, pieceState2 });

                // act
                var actual = gameState.CompareTo(gameState);

                // assert
                actual.ShouldBeEmpty();
            }

            [Fact]
            public void Should_throw_comparing_different_games()
            {
                // arrange
                var game1 = new Boards.Backgammon.BackgammonGameBuilder().Compile();
                var game2 = new Boards.Chess.ChessGameBuilder().Compile();
                var gameState1 = GameState.New(game1, null);
                var gameState2 = GameState.New(game2, null);

                // act
                var actual = Should.Throw<ArgumentException>(() => gameState1.CompareTo(gameState2));

                // assert
                actual.ParamName.ShouldBe("state");
            }

            [Fact]
            public void Should_single_addition()
            {
                // arrange
                var piece1 = Game.GetPiece("piece-1");
                var piece2 = Game.GetPiece("piece-2");
                var tile1 = Game.GetTile("tile-1");
                var tile2 = Game.GetTile("tile-2");
                var dice = new RegularDice("dice");
                var pieceState1 = new PieceState(piece1, tile1);
                var pieceState2 = new PieceState(piece2, tile2);
                var diceState = new DiceState<int>(dice, 5);
                var gameState1 = GameState.New(Game, new IArtifactState[] { diceState, pieceState1 });
                var gameState2 = gameState1.Next(new [] { pieceState2 });

                // act
                var actual = gameState2.CompareTo(gameState1);

                // assert
                actual.Count().ShouldBe(1);
                actual.Single().From.ShouldBe(null);
                actual.Single().To.ShouldBe(pieceState2);
            }

            [Fact]
            public void Should_single_change()
            {
                // arrange
                var piece1 = Game.GetPiece("piece-1");
                var piece2 = Game.GetPiece("piece-2");
                var tile1 = Game.GetTile("tile-1");
                var tile2 = Game.GetTile("tile-2");
                var dice = new RegularDice("dice");
                var pieceState1 = new PieceState(piece1, tile1);
                var pieceState2 = new PieceState(piece2, tile2);
                var diceState1 = new DiceState<int>(dice, 5);
                var diceState2 = new DiceState<int>(dice, 3);
                var gameState1 = GameState.New(Game, new IArtifactState[] { diceState1, pieceState1, pieceState2 });
                var gameState2 = gameState1.Next(new IArtifactState[] { pieceState2, diceState2, pieceState1 });

                // act
                var actual = gameState2.CompareTo(gameState1);

                // assert
                actual.Count().ShouldBe(1);
                actual.Single().From.ShouldBe(diceState1);
                actual.Single().To.ShouldBe(diceState2);
            }

            [Fact]
            public void Should_capture_various_updates()
            {
                // arrange
                var piece1 = Game.GetPiece("piece-1");
                var piece2 = Game.GetPiece("piece-2");
                var pieceN = Game.GetPiece("piece-n");
                var tile1 = Game.GetTile("tile-1");
                var tile2 = Game.GetTile("tile-2");
                var dice = new RegularDice("dice");
                var pieceState1 = new PieceState(piece1, tile1);
                var pieceState2 = new PieceState(piece2, tile2);
                var diceState1 = new DiceState<int>(dice, 5);
                var diceState2 = new DiceState<int>(dice, 3);
                var pieceState1new = new PieceState(piece1, tile2);
                var pieceStateN = new PieceState(pieceN, tile1);
                var gameState1 = GameState.New(Game, new IArtifactState[] { diceState1, pieceState1, pieceState2 });
                var gameState2 = gameState1.Next(new IArtifactState[] { pieceState2, diceState2, pieceState1new, pieceStateN });

                // act
                var actual = gameState2.CompareTo(gameState1);

                // assert
                actual.Count().ShouldBe(3);
                actual.Count(x => x.From == null && x.To.Equals(pieceStateN)).ShouldBe(1);
                actual.Count(x => pieceState1.Equals(x.From) && pieceState1new.Equals(x.To)).ShouldBe(1);
                actual.Count(x => diceState1.Equals(x.From) && diceState2.Equals(x.To)).ShouldBe(1);
            }
        }

        public class _Equals : GameStateTests
        {
            private GameState NewGameState(string tileId1 = "tile-1", string tileId2 = "tile-2", int diceValue = 3)
            {
                var piece1 = Game.GetPiece("piece-1");
                var piece2 = Game.GetPiece("piece-2");
                var tile1 = !string.IsNullOrEmpty(tileId1) ? Game.GetTile(tileId1) : null;
                var tile2 = !string.IsNullOrEmpty(tileId2) ? Game.GetTile(tileId2) : null;
                var dice = new RegularDice("dice");
                var pieceState1 = tile1 != null ? new PieceState(piece1, tile1) : null;
                var pieceState2 = tile2 != null ? new PieceState(piece2, tile2) : null;
                var diceState = new DiceState<int>(dice, diceValue);
                return GameState.New(Game, new IArtifactState[] { diceState, pieceState1, pieceState2 }.Where(x => x != null));
            }

            [Fact]
            public void Should_equal_self()
            {
                // arrange
                var gameState = NewGameState();

                // act
                var actual = gameState.Equals(gameState);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var gameState = NewGameState();

                // act
                var actual = gameState.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_similar_values_different_instance()
            {
                // arrange
                var gameState1 = NewGameState();
                var gameState2 = NewGameState();

                // act
                var actual = gameState1.Equals(gameState2);

                // assert
                gameState1.ShouldNotBeSameAs(gameState2);
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_equal_when_game_is_different()
            {
                // arrange
                var game1 = new Boards.Backgammon.BackgammonGameBuilder().Compile();
                var game2 = new Boards.Chess.ChessGameBuilder().Compile();
                var gameState1 = GameState.New(game1, null);
                var gameState2 = GameState.New(game2, null);

                // act
                var actual = gameState1.Equals(gameState2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_similar_values_but_not_initial()
            {
                // arrange
                var gameState1 = NewGameState();
                var gameState2 = NewGameState().Next(Enumerable.Empty<IArtifactState>());

                // act
                var actual = gameState1.Equals(gameState2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_one_state_different()
            {
                // arrange
                var gameState1 = NewGameState("tile-1", "tile-2", 3);
                var gameState2 = NewGameState("tile-1", "tile-2", 4);

                // act
                var actual = gameState1.Equals(gameState2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_state_count()
            {
                // arrange
                var gameState1 = NewGameState("tile-1", "tile-2", 3);
                var gameState2 = NewGameState("tile-1", null, 3);

                // act
                var actual = gameState1.Equals(gameState2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_type()
            {
                // arrange
                var gameState = NewGameState();

                // act
                var actual = gameState.Equals("a dumb string");

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_same_states_different_order()
            {
                // arrange
                var piece1 = Game.GetPiece("piece-1");
                var piece2 = Game.GetPiece("piece-2");
                var tile1 = Game.GetTile("tile-1");
                var tile2 = Game.GetTile("tile-2");
                var dice = new RegularDice("dice");
                var pieceState1 = new PieceState(piece1, tile1);
                var pieceState2 = new PieceState(piece2, tile2);
                var diceState = new DiceState<int>(dice, 3);
                var gameState1 = GameState.New(Game, new IArtifactState[] { diceState, pieceState1, pieceState2 });
                var gameState2 = GameState.New(Game, new IArtifactState[] { pieceState2, diceState, pieceState1 });

                // act
                var actual = gameState1.Equals(gameState2);

                // assert
                actual.ShouldBeTrue();
            }
        }

        public class _GetHashCode : GameStateTests
        {
            [Fact]
            public void Should_equal_self()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var piece = game.GetPiece("piece-1");
                var tile = game.GetTile("tile-1");
                var dice = game.GetArtifact<RegularDice>("dice");
                var state1 = new PieceState(piece, tile);
                var state2 = new DiceState<int>(dice, 4);
                var gameState = GameState.New(game, new IArtifactState[] { state1, state2 });
                var expected = (((typeof(GameState).GetHashCode() * 397) ^ game.GetHashCode()) * 397) ^ true.GetHashCode() ^ state1.GetHashCode() ^ state2.GetHashCode();

                // act
                var actual = gameState.GetHashCode();

                // assert
                actual.ShouldBe(expected);
            }

            [Fact]
            public void Should_return_same_hashcode_different_order_states()
            {
                // arrange
                var piece1 = Game.GetPiece("piece-1");
                var piece2 = Game.GetPiece("piece-2");
                var tile1 = Game.GetTile("tile-1");
                var tile2 = Game.GetTile("tile-2");
                var dice = new RegularDice("dice");
                var pieceState1 = new PieceState(piece1, tile1);
                var pieceState2 = new PieceState(piece2, tile2);
                var diceState = new DiceState<int>(dice, 3);
                var gameState1 = GameState.New(Game, new IArtifactState[] { diceState, pieceState1, pieceState2 });
                var gameState2 = GameState.New(Game, new IArtifactState[] { pieceState2, diceState, pieceState1 });

                // act
                var actual = gameState1.GetHashCode() == gameState2.GetHashCode();

                // assert
                actual.ShouldBeTrue();
            }
        }
    }
}