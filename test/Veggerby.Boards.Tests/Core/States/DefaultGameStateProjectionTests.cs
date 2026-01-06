using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class DefaultGameStateProjectionTests
{
    public Game Game { get; }

    public DefaultGameStateProjectionTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    public class Constructor : DefaultGameStateProjectionTests
    {
        [Fact]
        public void Should_Create_Projection_With_State()
        {
            // arrange
            var gameState = GameState.New([]);

            // act
            var projection = new DefaultGameStateProjection(gameState);

            // assert
            projection.Should().NotBeNull();
        }

        [Fact]
        public void Should_Create_Projection_With_State_And_Policy()
        {
            // arrange
            var gameState = GameState.New([]);
            var policy = FullVisibilityPolicy.Instance;

            // act
            var projection = new DefaultGameStateProjection(gameState, policy);

            // assert
            projection.Should().NotBeNull();
        }

        [Fact]
        public void Should_Use_Full_Visibility_Policy_When_Null()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var player = Game.GetPlayer("player-1").EnsureNotNull();

            // act
            var projection = new DefaultGameStateProjection(gameState, policy: null);
            var view = projection.ProjectFor(player);

            // assert
            view.VisibleStates.Should().HaveCount(1);
        }

        [Fact]
        public void Should_Throw_When_State_Is_Null()
        {
            // arrange & act
            var act = () => new DefaultGameStateProjection(null!);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }
    }

    public class ProjectFor : DefaultGameStateProjectionTests
    {
        [Fact]
        public void Should_Create_Player_View()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var projection = new DefaultGameStateProjection(gameState);

            // act
            var view = projection.ProjectFor(player);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().HaveCount(1);
        }

        [Fact]
        public void Should_Throw_When_Player_Is_Null()
        {
            // arrange
            var gameState = GameState.New([]);
            var projection = new DefaultGameStateProjection(gameState);

            // act
            var act = () => projection.ProjectFor(null!);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Apply_Configured_Policy()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var policy = FullVisibilityPolicy.Instance;
            var projection = new DefaultGameStateProjection(gameState, policy);

            // act
            var view = projection.ProjectFor(player);

            // assert
            view.VisibleStates.Should().HaveCount(1);
            view.VisibleStates.Should().Contain(state);
        }
    }

    public class ProjectForObserver : DefaultGameStateProjectionTests
    {
        [Fact]
        public void Should_Create_Full_Observer_View()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var projection = new DefaultGameStateProjection(gameState);

            // act
            var view = projection.ProjectForObserver(ObserverRole.Full);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().HaveCount(1);
            view.VisibleStates.Should().Contain(state);
        }

        [Fact]
        public void Should_Create_Limited_Observer_View()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var projection = new DefaultGameStateProjection(gameState);

            // act
            var view = projection.ProjectForObserver(ObserverRole.Limited);

            // assert
            view.Should().NotBeNull();
            // Limited observer should see public states only (PieceState defaults to Public)
            view.VisibleStates.Should().HaveCount(1);
        }

        [Fact]
        public void Should_Create_Player_Perspective_Observer_View()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var gameState = GameState.New([state]);
            var projection = new DefaultGameStateProjection(gameState);

            // act
            var view = projection.ProjectForObserver(ObserverRole.PlayerPerspective);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().HaveCount(1);
        }

        [Fact]
        public void Should_Throw_For_Unknown_Observer_Role()
        {
            // arrange
            var gameState = GameState.New([]);
            var projection = new DefaultGameStateProjection(gameState);

            // act
            var act = () => projection.ProjectForObserver((ObserverRole)999);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
