using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class FullVisibilityPolicyTests
{
    public Game Game { get; }

    public FullVisibilityPolicyTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    [Fact]
    public void Instance_Should_Return_Singleton()
    {
        // arrange & act
        var instance1 = FullVisibilityPolicy.Instance;
        var instance2 = FullVisibilityPolicy.Instance;

        // assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void CanSee_Should_Always_Return_True()
    {
        // arrange
        var policy = FullVisibilityPolicy.Instance;
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new PieceState(piece, tile);
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var result = policy.CanSee(player, state);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanSee_Should_Return_True_For_Null_Viewer()
    {
        // arrange
        var policy = FullVisibilityPolicy.Instance;
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new PieceState(piece, tile);

        // act
        var result = policy.CanSee(viewer: null, state);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanSee_Should_Throw_When_State_Is_Null()
    {
        // arrange
        var policy = FullVisibilityPolicy.Instance;
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var act = () => policy.CanSee(player, null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Redact_Should_Return_Original_State()
    {
        // arrange
        var policy = FullVisibilityPolicy.Instance;
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
    public void Redact_Should_Return_Original_State_For_Null_Viewer()
    {
        // arrange
        var policy = FullVisibilityPolicy.Instance;
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new PieceState(piece, tile);

        // act
        var result = policy.Redact(viewer: null, state);

        // assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public void Redact_Should_Throw_When_State_Is_Null()
    {
        // arrange
        var policy = FullVisibilityPolicy.Instance;
        var player = Game.GetPlayer("player-1").EnsureNotNull();

        // act
        var act = () => policy.Redact(player, null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}
