using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class GameStateViewTests
{
    public Game Game { get; }

    public GameStateViewTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    public class Constructor : GameStateViewTests
    {
        [Fact]
        public void Should_Create_View_With_Valid_Parameters()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();

            // act
            var view = new GameStateView(gameState, policy, player);

            // assert
            view.Should().NotBeNull();
        }

        [Fact]
        public void Should_Throw_When_State_Is_Null()
        {
            // arrange
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();

            // act
            var act = () => new GameStateView(null!, policy, player);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Throw_When_Policy_Is_Null()
        {
            // arrange
            var gameState = GameState.New([]);
            var player = Game.GetPlayer("player-1").EnsureNotNull();

            // act
            var act = () => new GameStateView(gameState, null!, player);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Allow_Null_Viewer_For_Observer_Contexts()
        {
            // arrange
            var gameState = GameState.New([]);
            var policy = FullVisibilityPolicy.Instance;

            // act
            var view = new GameStateView(gameState, policy, viewer: null);

            // assert
            view.Should().NotBeNull();
        }
    }

    public class VisibleStates : GameStateViewTests
    {
        [Fact]
        public void Should_Return_All_States_With_Full_Visibility_Policy()
        {
            // arrange
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var state1 = new PieceState(piece1, tile1);
            var state2 = new PieceState(piece2, tile2);
            var gameState = GameState.New([state1, state2]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();

            // act
            var view = new GameStateView(gameState, policy, player);

            // assert
            view.VisibleStates.Should().HaveCount(2);
            view.VisibleStates.Should().Contain(state1);
            view.VisibleStates.Should().Contain(state2);
        }

        [Fact]
        public void Should_Return_Empty_When_No_States()
        {
            // arrange
            var gameState = GameState.New([]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();

            // act
            var view = new GameStateView(gameState, policy, player);

            // assert
            view.VisibleStates.Should().BeEmpty();
        }

        [Fact]
        public void Should_Cache_Visible_States()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var view = new GameStateView(gameState, policy, player);

            // act
            var result1 = view.VisibleStates;
            var result2 = view.VisibleStates;

            // assert
            result1.Should().BeSameAs(result2);
        }
    }

    public class GetState : GameStateViewTests
    {
        [Fact]
        public void Should_Return_State_When_Visible()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var expected = new PieceState(piece, tile);
            var gameState = GameState.New([expected]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var view = new GameStateView(gameState, policy, player);

            // act
            var actual = view.GetState<PieceState>(piece);

            // assert
            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [Fact]
        public void Should_Return_Null_When_State_Not_Present()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var gameState = GameState.New([]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var view = new GameStateView(gameState, policy, player);

            // act
            var actual = view.GetState<PieceState>(piece);

            // assert
            actual.Should().BeNull();
        }

        [Fact]
        public void Should_Return_Null_When_Type_Mismatch()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var view = new GameStateView(gameState, policy, player);

            // act
            var actual = view.GetState<ActivePlayerState>(piece);

            // assert
            actual.Should().BeNull();
        }
    }

    public class GetStates : GameStateViewTests
    {
        [Fact]
        public void Should_Return_All_States_Of_Type()
        {
            // arrange
            var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
            var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
            var tile1 = Game.GetTile("tile-1").EnsureNotNull();
            var tile2 = Game.GetTile("tile-2").EnsureNotNull();
            var state1 = new PieceState(piece1, tile1);
            var state2 = new PieceState(piece2, tile2);
            var gameState = GameState.New([state1, state2]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var view = new GameStateView(gameState, policy, player);

            // act
            var actual = view.GetStates<PieceState>();

            // assert
            actual.Should().HaveCount(2);
            actual.Should().Contain(state1);
            actual.Should().Contain(state2);
        }

        [Fact]
        public void Should_Return_Empty_When_No_Matching_Type()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var view = new GameStateView(gameState, policy, player);

            // act
            var actual = view.GetStates<ActivePlayerState>();

            // assert
            actual.Should().BeEmpty();
        }
    }

    public class UnsafeFullState : GameStateViewTests
    {
        [Fact]
        public void Should_Return_Underlying_State()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var policy = FullVisibilityPolicy.Instance;
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var view = new GameStateView(gameState, policy, player);

            // act
            var actual = view.UnsafeFullState;

            // assert
            actual.Should().BeSameAs(gameState);
        }
    }
}
