using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class GameStateTests
{
    public Game Game { get; }
    public GameStateTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    public class New : GameStateTests
    {
        [Fact]
        public void Should_create_new_game_state_with_empty()
        {
            // arrange

            // act
            var actual = GameState.New(Array.Empty<IArtifactState>());

            // assert
            actual.Should().NotBeNull();
            actual.IsInitialState.Should().BeTrue();
            actual.ChildStates.Should().BeEmpty();
        }

        [Fact]
        public void Should_create_new_game_state_child_states()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state1 = new PieceState(piece, tile); // unused state used just to assert creation

            // act
            var actual = GameState.New(Array.Empty<IArtifactState>());

            // assert
            actual.Should().NotBeNull();
            actual.IsInitialState.Should().BeTrue();
            actual.ChildStates.Should().BeEmpty();
        }
    }

    public class GetState : GameStateTests
    {
        [Fact]
        public void Should_return_artifact_get_state()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var expected = new PieceState(piece, tile);
            var gameState = GameState.New([expected]);

            // act
            var actual = gameState.GetState<PieceState>(piece);

            // assert
            actual.Should().NotBeNull();
            actual.Should().BeOfType<PieceState>();
            actual.Artifact.Should().Be(piece);
        }
    }

    public class IsInitialState : GameStateTests
    {
        [Fact]
        public void Should_return_initial_state()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var expected = new PieceState(piece, tile);
            var gameState = GameState.New([expected]);

            // act
            var actual = gameState.IsInitialState;

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_return_initial_state()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var expected = new PieceState(piece, tile);
            var gameState = GameState.New(Array.Empty<IArtifactState>())
                .Next([expected]);

            // act
            var actual = gameState.IsInitialState;

            // assert
            actual.Should().BeFalse();
        }
    }

    public class Next : GameStateTests
    {
        [Fact]
        public void Should_return_next_state()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var dice = new Dice("dice");
            var pieceState1 = new PieceState(piece, tile1);
            var pieceState2 = new PieceState(piece, tile2);
            var diceState = new DiceState<int>(dice, 5);
            var gameState = GameState.New([diceState, pieceState1]);

            // act
            var actual = gameState.Next([pieceState2]);

            // assert
            actual.Should().NotBeNull();
            actual.IsInitialState.Should().BeFalse();
            actual.ChildStates.Should().Equal([diceState, pieceState2]);
        }
    }

    public class CompareTo : GameStateTests
    {
        [Fact]
        public void Should_return_no_changes_when_equals()
        {
            // arrange
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var dice = new Dice("dice");
            var pieceState1 = new PieceState(piece1, tile1);
            var pieceState2 = new PieceState(piece2, tile2);
            var diceState = new DiceState<int>(dice, 5);
            var gameState = GameState.New([diceState, pieceState1, pieceState2]);

            // act
            var actual = gameState.CompareTo(gameState);

            // assert
            actual.Should().BeEmpty();
        }

        [Fact]
        public void Should_single_addition()
        {
            // arrange
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var dice = new Dice("dice");
            var pieceState1 = new PieceState(piece1, tile1);
            var pieceState2 = new PieceState(piece2, tile2);
            var diceState = new DiceState<int>(dice, 5);
            var gameState1 = GameState.New([diceState, pieceState1]);
            var gameState2 = gameState1.Next([pieceState2]);

            // act
            var actual = gameState2.CompareTo(gameState1);

            // assert
            actual.Count().Should().Be(1);
            actual.Single().From.Should().Be(null);
            actual.Single().To.Should().Be(pieceState2);
        }

        [Fact]
        public void Should_single_change()
        {
            // arrange
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var dice = new Dice("dice");
            var pieceState1 = new PieceState(piece1, tile1);
            var pieceState2 = new PieceState(piece2, tile2);
            var diceState1 = new DiceState<int>(dice, 5);
            var diceState2 = new DiceState<int>(dice, 3);
            var gameState1 = GameState.New([diceState1, pieceState1, pieceState2]);
            var gameState2 = gameState1.Next([pieceState2, diceState2, pieceState1]);

            // act
            var actual = gameState2.CompareTo(gameState1);

            // assert
            actual.Count().Should().Be(1);
            actual.Single().From.Should().Be(diceState1);
            actual.Single().To.Should().Be(diceState2);
        }

        [Fact]
        public void Should_capture_various_updates()
        {
            // arrange
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var pieceN = Game.GetPiece("piece-n").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var dice = new Dice("dice");
            var pieceState1 = new PieceState(piece1, tile1);
            var pieceState2 = new PieceState(piece2, tile2);
            var diceState1 = new DiceState<int>(dice, 5);
            var diceState2 = new DiceState<int>(dice, 3);
            var pieceState1new = new PieceState(piece1, tile2);
            var pieceStateN = new PieceState(pieceN, tile1);
            var gameState1 = GameState.New([diceState1, pieceState1, pieceState2]);
            var gameState2 = gameState1.Next([pieceState2, diceState2, pieceState1new, pieceStateN]);

            // act
            var actual = gameState2.CompareTo(gameState1);

            // assert
            actual.Count().Should().Be(3);
            actual.Count(x => x.From is null && x.To is not null && x.To.Equals(pieceStateN)).Should().Be(1);
            actual.Count(x => x.From is not null && x.To is not null && pieceState1.Equals(x.From) && pieceState1new.Equals(x.To)).Should().Be(1);
            actual.Count(x => x.From is not null && x.To is not null && diceState1.Equals(x.From) && diceState2.Equals(x.To)).Should().Be(1);
        }
    }

    public class _Equals : GameStateTests
    {
        private GameState NewGameState(string? tileId1 = "tile-1", string? tileId2 = "tile-2", int diceValue = 3)
        {
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var tile1Maybe = !string.IsNullOrEmpty(tileId1) ? Game.GetTile(tileId1!).EnsureNotNull() : null;
            var tile2Maybe = !string.IsNullOrEmpty(tileId2) ? Game.GetTile(tileId2!).EnsureNotNull() : null;
            var dice = new Dice("dice");
            var states = new System.Collections.Generic.List<IArtifactState>
            {
                new DiceState<int>(dice, diceValue)
            };
            if (tile1Maybe is not null)
            {
                states.Add(new PieceState(piece1, tile1Maybe));
            }
            if (tile2Maybe is not null)
            {
                states.Add(new PieceState(piece2, tile2Maybe));
            }

            return GameState.New(states);
        }

        [Fact]
        public void Should_equal_self()
        {
            // arrange
            var gameState = NewGameState();

            // act
            var actual = gameState.Equals(gameState);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange
            var gameState = NewGameState();

            // act
            var actual = gameState.Equals(null);

            // assert
            actual.Should().BeFalse();
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
            gameState1.Should().NotBeSameAs(gameState2);
            actual.Should().BeTrue();
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
            actual.Should().BeFalse();
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
            actual.Should().BeFalse();
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
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_different_type()
        {
            // arrange
            var gameState = NewGameState();

            // act
            var actual = gameState.Equals("a dumb string");

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_same_states_different_order()
        {
            // arrange
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var dice = new Dice("dice");
            var pieceState1 = new PieceState(piece1, tile1);
            var pieceState2 = new PieceState(piece2, tile2);
            var diceState = new DiceState<int>(dice, 3);
            var gameState1 = GameState.New([diceState, pieceState1, pieceState2]);
            var gameState2 = GameState.New([pieceState2, diceState, pieceState1]);

            // act
            var actual = gameState1.Equals(gameState2);

            // assert
            actual.Should().BeTrue();
        }
    }
}