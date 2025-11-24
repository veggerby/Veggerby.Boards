using Veggerby.Boards;
using Veggerby.Boards.Checkers;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersBasicMovementTests
{
    [Fact]
    public void Should_allow_black_piece_to_move_forward_diagonally_southwest()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // act - black piece from tile 9 to tile 13 (southwest)
        var result = progress.Move("black-piece-9", "tile-13");

        // assert
        var pieceState = result.State.GetStates<PieceState>()
            .Should().ContainSingle(ps => ps.Artifact.Id == "black-piece-9")
            .Which;

        pieceState.CurrentTile.Id.Should().Be("tile-13");
    }

    [Fact]
    public void Should_allow_black_piece_to_move_forward_diagonally_southeast()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // act - black piece from tile 9 to tile 14 (southeast)
        var result = progress.Move("black-piece-9", "tile-14");

        // assert
        var pieceState = result.State.GetStates<PieceState>()
            .Should().ContainSingle(ps => ps.Artifact.Id == "black-piece-9")
            .Which;

        pieceState.CurrentTile.Id.Should().Be("tile-14");
    }

    [Fact]
    public void Should_allow_white_piece_to_move_forward_diagonally_northeast()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // Move black first to switch to white
        progress = progress.Move("black-piece-9", "tile-13");

        // act - white piece from tile 21 to tile 17 (northeast)
        var result = progress.Move("white-piece-1", "tile-17");

        // assert
        var pieceState = result.State.GetStates<PieceState>()
            .Should().ContainSingle(ps => ps.Artifact.Id == "white-piece-1")
            .Which;

        pieceState.CurrentTile.Id.Should().Be("tile-17");
    }

    [Fact]
    public void Should_allow_white_piece_to_move_forward_diagonally_northwest()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // Move black first to switch to white
        progress = progress.Move("black-piece-9", "tile-13");

        // act - white piece from tile 22 to tile 18 (northwest)
        var result = progress.Move("white-piece-2", "tile-18");

        // assert
        var pieceState = result.State.GetStates<PieceState>()
            .Should().ContainSingle(ps => ps.Artifact.Id == "white-piece-2")
            .Which;

        pieceState.CurrentTile.Id.Should().Be("tile-18");
    }

    [Fact]
    public void Should_alternate_players_after_each_move()
    {
        // arrange
        var progress = new CheckersGameBuilder().Compile();

        // assert initial - black is active
        progress.State.TryGetActivePlayer(out var player1);
        player1!.Id.Should().Be(CheckersIds.Players.Black);

        // act - black moves
        progress = progress.Move("black-piece-9", "tile-13");

        // assert - white is now active
        progress.State.TryGetActivePlayer(out var player2);
        player2!.Id.Should().Be(CheckersIds.Players.White);

        // act - white moves
        progress = progress.Move("white-piece-1", "tile-17");

        // assert - black is active again
        progress.State.TryGetActivePlayer(out var player3);
        player3!.Id.Should().Be(CheckersIds.Players.Black);
    }
}
