using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class PlayerOwnedVisibilityPolicyTests
{
    public Game Game { get; }

    public PlayerOwnedVisibilityPolicyTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    [Fact]
    public void Instance_Should_Return_Singleton()
    {
        // arrange & act
        var instance1 = PlayerOwnedVisibilityPolicy.Instance;
        var instance2 = PlayerOwnedVisibilityPolicy.Instance;

        // assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void CanSee_Should_Throw_When_State_Is_Null()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var act = () => policy.CanSee(player, null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CanSee_Should_Return_True_For_Public_State()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new PieceState(piece, tile); // Default visibility is Public
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var result = policy.CanSee(player, state);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanSee_Should_Return_False_For_Hidden_State()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new TestPrivateState(piece, tile, Visibility.Hidden);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var result = policy.CanSee(player, state);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanSee_Should_Return_True_For_Private_State_Owned_By_Viewer()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var player1 = Game.GetPlayer("player-1").EnsureNotNull();
        var piece = Game.GetPiece("piece-1").EnsureNotNull(); // Owned by player-1
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new TestPrivateState(piece, tile, Visibility.Private);

        // act
        var result = policy.CanSee(player1, state);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanSee_Should_Return_False_For_Private_State_Not_Owned_By_Viewer()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var player2 = Game.GetPlayer("player-2").EnsureNotNull();
        var piece = Game.GetPiece("piece-1").EnsureNotNull(); // Owned by player-1
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new TestPrivateState(piece, tile, Visibility.Private);

        // act
        var result = policy.CanSee(player2, state);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Redact_Should_Return_Original_State_When_Visible()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var player1 = Game.GetPlayer("player-1").EnsureNotNull();
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new PieceState(piece, tile);

        // act
        var result = policy.Redact(player1, state);

        // assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public void Redact_Should_Return_RedactedPieceState_For_Hidden_PieceState()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var player2 = Game.GetPlayer("player-2").EnsureNotNull();
        var piece = Game.GetPiece("piece-1").EnsureNotNull(); // Owned by player-1
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new TestPrivateState(piece, tile, Visibility.Private);

        // act
        var result = policy.Redact(player2, state);

        // assert
        result.Should().BeOfType<RedactedPieceState>();
        var redacted = result as RedactedPieceState;
        redacted!.Artifact.Should().Be(piece);
        redacted.CurrentTile.Should().Be(tile);
    }

    [Fact]
    public void Redact_Should_Return_Null_For_Non_PieceState_Hidden()
    {
        // arrange
        var policy = PlayerOwnedVisibilityPolicy.Instance;
        var player = Game.GetPlayer("player-1").EnsureNotNull();
        var testArtifact = new TestArtifact("test-artifact");
        var state = new TestNonPiecePrivateState(testArtifact, Visibility.Hidden);

        // act
        var result = policy.Redact(player, state);

        // assert
        result.Should().BeNull();
    }

    // Helper test states
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

    private class TestNonPiecePrivateState : IArtifactState
    {
        private readonly Visibility _visibility;

        public TestNonPiecePrivateState(Artifact artifact, Visibility visibility)
        {
            Artifact = artifact;
            _visibility = visibility;
        }

        public Artifact Artifact { get; }
        Visibility IArtifactState.Visibility => _visibility;

        public bool Equals(IArtifactState? other) => other is TestNonPiecePrivateState t && t.Artifact.Equals(Artifact);
        public override bool Equals(object? obj) => Equals(obj as IArtifactState);
        public override int GetHashCode() => Artifact.GetHashCode();
    }
}
