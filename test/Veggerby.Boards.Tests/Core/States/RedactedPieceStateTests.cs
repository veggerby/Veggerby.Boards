using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.States;

public class RedactedPieceStateTests
{
    public Game Game { get; }

    public RedactedPieceStateTests()
    {
        Game = new TestGameBuilder().Compile().Game;
    }

    [Fact]
    public void Constructor_Should_Create_RedactedPieceState()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();

        // act
        var state = new RedactedPieceState(piece, tile);

        // assert
        state.Should().NotBeNull();
        state.Artifact.Should().Be(piece);
        state.CurrentTile.Should().Be(tile);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Piece_Is_Null()
    {
        // arrange
        var tile = Game.GetTile("tile-1").EnsureNotNull();

        // act
        var act = () => new RedactedPieceState(null!, tile);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Tile_Is_Null()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();

        // act
        var act = () => new RedactedPieceState(piece, null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Visibility_Should_Default_To_Public()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new RedactedPieceState(piece, tile);

        // act
        var visibility = (state as IArtifactState).Visibility;

        // assert
        visibility.Should().Be(Visibility.Public);
    }

    [Fact]
    public void Equals_Should_Return_True_For_Same_Artifact_And_Tile()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state1 = new RedactedPieceState(piece, tile);
        var state2 = new RedactedPieceState(piece, tile);

        // act
        var result = state1.Equals(state2);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_For_Different_Tile()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile1 = Game.GetTile("tile-1").EnsureNotNull();
        var tile2 = Game.GetTile("tile-2").EnsureNotNull();
        var state1 = new RedactedPieceState(piece, tile1);
        var state2 = new RedactedPieceState(piece, tile2);

        // act
        var result = state1.Equals(state2);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_For_Different_Piece()
    {
        // arrange
        var piece1 = Game.GetPiece("piece-1").EnsureNotNull();
        var piece2 = Game.GetPiece("piece-2").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state1 = new RedactedPieceState(piece1, tile);
        var state2 = new RedactedPieceState(piece2, tile);

        // act
        var result = state1.Equals(state2);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_For_Null()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new RedactedPieceState(piece, tile);

        // act
        var result = state.Equals(null);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_Be_Consistent()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state1 = new RedactedPieceState(piece, tile);
        var state2 = new RedactedPieceState(piece, tile);

        // act
        var hash1 = state1.GetHashCode();
        var hash2 = state2.GetHashCode();

        // assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ToString_Should_Return_Descriptive_String()
    {
        // arrange
        var piece = Game.GetPiece("piece-1").EnsureNotNull();
        var tile = Game.GetTile("tile-1").EnsureNotNull();
        var state = new RedactedPieceState(piece, tile);

        // act
        var result = state.ToString();

        // assert
        result.Should().Contain("RedactedPiece");
        result.Should().Contain(tile.Id);
    }
}
