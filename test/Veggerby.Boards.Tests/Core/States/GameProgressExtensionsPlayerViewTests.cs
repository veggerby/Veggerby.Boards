using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class GameProgressExtensionsPlayerViewTests
{
    public Game Game { get; }

    public GameProgressExtensionsPlayerViewTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    public class GetViewFor : GameProgressExtensionsPlayerViewTests
    {
        [Fact]
        public void Should_Create_Player_View_With_Default_Policy()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var progress = new TestGameBuilder().Compile();
            var progressWithState = progress.NewState([state]);

            // act
            var view = progressWithState.GetViewFor(player);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().NotBeEmpty();
            view.GetState<PieceState>(piece).Should().Be(state);
        }

        [Fact]
        public void Should_Create_Player_View_With_Custom_Policy()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var policy = FullVisibilityPolicy.Instance;
            var progress = new TestGameBuilder().Compile();
            var progressWithState = progress.NewState([state]);

            // act
            var view = progressWithState.GetViewFor(player, policy);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().NotBeEmpty();
            view.GetState<PieceState>(piece).Should().Be(state);
        }

        [Fact]
        public void Should_Throw_When_Progress_Is_Null()
        {
            // arrange
            var player = Game.GetPlayer("player-1").EnsureNotNull();

            // act
            var act = () => GameProgressExtensions.GetViewFor(null!, player);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Throw_When_Player_Is_Null()
        {
            // arrange
            var progress = new TestGameBuilder().Compile();

            // act
            var act = () => progress.GetViewFor(null!);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Return_Consistent_View_For_Same_Player()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var player = Game.GetPlayer("player-1").EnsureNotNull();
            var progress = new TestGameBuilder().Compile();
            var progressWithState = progress.NewState([state]);

            // act
            var view1 = progressWithState.GetViewFor(player);
            var view2 = progressWithState.GetViewFor(player);

            // assert
            view1.VisibleStates.Should().BeEquivalentTo(view2.VisibleStates);
        }
    }

    public class GetObserverView : GameProgressExtensionsPlayerViewTests
    {
        [Fact]
        public void Should_Create_Full_Observer_View()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var progress = new TestGameBuilder().Compile();
            var progressWithState = progress.NewState([state]);

            // act
            var view = progressWithState.GetObserverView(ObserverRole.Full);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().NotBeEmpty();
            view.GetState<PieceState>(piece).Should().Be(state);
        }

        [Fact]
        public void Should_Create_Limited_Observer_View()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var progress = new TestGameBuilder().Compile();
            var progressWithState = progress.NewState([state]);

            // act
            var view = progressWithState.GetObserverView(ObserverRole.Limited);

            // assert
            view.Should().NotBeNull();
            // Limited observer should see public states (PieceState defaults to Public)
            view.VisibleStates.Should().NotBeEmpty();
            view.GetState<PieceState>(piece).Should().Be(state);
        }

        [Fact]
        public void Should_Create_Player_Perspective_Observer_View()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var progress = new TestGameBuilder().Compile();
            var progressWithState = progress.NewState([state]);

            // act
            var view = progressWithState.GetObserverView(ObserverRole.PlayerPerspective);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().NotBeEmpty();
            view.GetState<PieceState>(piece).Should().Be(state);
        }

        [Fact]
        public void Should_Throw_When_Progress_Is_Null()
        {
            // arrange & act
            var act = () => GameProgressExtensions.GetObserverView(null!, ObserverRole.Full);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Support_Custom_Policy_For_Observers()
        {
            // arrange
            var piece = Game.GetPiece("piece-1").EnsureNotNull();
            var tile = Game.GetTile("tile-1").EnsureNotNull();
            var state = new PieceState(piece, tile);
            var policy = FullVisibilityPolicy.Instance;
            var progress = new TestGameBuilder().Compile();
            var progressWithState = progress.NewState([state]);

            // act
            var view = progressWithState.GetObserverView(ObserverRole.Limited, policy);

            // assert
            view.Should().NotBeNull();
            view.VisibleStates.Should().NotBeEmpty();
            view.GetState<PieceState>(piece).Should().Be(state);
        }
    }
}
