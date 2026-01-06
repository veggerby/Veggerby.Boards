using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class ObserverVisibilityPolicyTests
{
    public Game Game { get; }

    public ObserverVisibilityPolicyTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    [Fact]
    public void Constructor_Should_Create_Policy_With_Role()
    {
        // arrange & act
        var policy = new ObserverVisibilityPolicy(ObserverRole.Full);

        // assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Should_Accept_Custom_Player_Policy()
    {
        // arrange
        var customPolicy = FullVisibilityPolicy.Instance;

        // act
        var policy = new ObserverVisibilityPolicy(ObserverRole.PlayerPerspective, customPolicy);

        // assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CanSee_Should_Throw_When_State_Is_Null()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.Full);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var act = () => policy.CanSee(player, null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CanSee_Full_Should_See_All_States()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.Full);
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var publicState = new PieceState(piece, tile);
        var privateState = new TestPrivateState(piece, tile, Visibility.Private);
        var hiddenState = new TestPrivateState(piece, tile, Visibility.Hidden);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act & assert
        policy.CanSee(player, publicState).Should().BeTrue();
        policy.CanSee(player, privateState).Should().BeTrue();
        policy.CanSee(player, hiddenState).Should().BeTrue();
    }

    [Fact]
    public void CanSee_Limited_Should_See_Only_Public_States()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.Limited);
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var publicState = new PieceState(piece, tile);
        var privateState = new TestPrivateState(piece, tile, Visibility.Private);
        var hiddenState = new TestPrivateState(piece, tile, Visibility.Hidden);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act & assert
        policy.CanSee(player, publicState).Should().BeTrue();
        policy.CanSee(player, privateState).Should().BeFalse();
        policy.CanSee(player, hiddenState).Should().BeFalse();
    }

    [Fact]
    public void CanSee_PlayerPerspective_Should_Delegate_To_Player_Policy()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.PlayerPerspective);
        var player1 = Game.GetPlayer("player-1").EnsureNotNull();
        var player2 = Game.GetPlayer("player-2").EnsureNotNull();
        var piece1 = Game.GetPiece("piece-1").EnsureNotNull(); // Owned by player-1
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var privateState = new TestPrivateState(piece1, tile, Visibility.Private);

        // act
        var canSeeAsOwner = policy.CanSee(player1, privateState);
        var canSeeAsNonOwner = policy.CanSee(player2, privateState);

        // assert
        canSeeAsOwner.Should().BeTrue(); // Owner can see
        canSeeAsNonOwner.Should().BeFalse(); // Non-owner cannot see
    }

    [Fact]
    public void CanSee_Should_Throw_For_Unknown_Role()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy((ObserverRole)999);
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new PieceState(piece, tile);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var act = () => policy.CanSee(player, state);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Redact_Full_Should_Return_Original_State()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.Full);
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new TestPrivateState(piece, tile, Visibility.Hidden);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var result = policy.Redact(player, state);

        // assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public void Redact_Limited_Should_Return_Null_For_Non_Public()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.Limited);
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var privateState = new TestPrivateState(piece, tile, Visibility.Private);
        var hiddenState = new TestPrivateState(piece, tile, Visibility.Hidden);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var privateResult = policy.Redact(player, privateState);
        var hiddenResult = policy.Redact(player, hiddenState);

        // assert
        privateResult.Should().BeNull();
        hiddenResult.Should().BeNull();
    }

    [Fact]
    public void Redact_Limited_Should_Return_Original_For_Public()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.Limited);
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new PieceState(piece, tile);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var result = policy.Redact(player, state);

        // assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public void Redact_PlayerPerspective_Should_Delegate_To_Player_Policy()
    {
        // arrange
        var policy = new ObserverVisibilityPolicy(ObserverRole.PlayerPerspective);
        var player2 = Game.GetPlayer("player-2").EnsureNotNull();
        var piece1 = Game.GetPiece("piece-1").EnsureNotNull(); // Owned by player-1
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var privateState = new TestPrivateState(piece1, tile, Visibility.Private);

        // act
        var result = policy.Redact(player2, privateState);

        // assert
        result.Should().BeOfType<RedactedPieceState>();
    }

    // Helper test state
    private class TestPrivateState : PieceState, IArtifactState
    {
        private readonly Visibility _visibility;

        public TestPrivateState(Piece piece, Tile tile, Visibility visibility)
            : base(piece, tile)
        {
            _visibility = visibility;
        }

        Visibility IArtifactState.Visibility => _visibility;
    }
}
