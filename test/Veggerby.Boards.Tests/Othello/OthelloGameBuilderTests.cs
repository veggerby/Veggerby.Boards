using System.Linq;

using Veggerby.Boards.Othello;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Othello;

public class OthelloGameBuilderTests
{
    [Fact]
    public void Should_create_game_with_8x8_board()
    {
        // arrange
        var builder = new OthelloGameBuilder();

        // act
        var progress = builder.Compile();

        // assert
        progress.Game.Board.Tiles.Count().Should().Be(64);
    }

    [Fact]
    public void Should_start_with_four_discs_in_center()
    {
        // arrange
        var builder = new OthelloGameBuilder();

        // act
        var progress = builder.Compile();

        // assert
        var pieceStates = progress.State.GetStates<PieceState>().ToList();
        pieceStates.Should().HaveCount(4);
    }

    [Fact]
    public void Should_have_correct_starting_position()
    {
        // arrange
        var builder = new OthelloGameBuilder();

        // act
        var progress = builder.Compile();

        // assert
        var d4Pieces = progress.State.GetPiecesOnTile(progress.Game.GetTile(OthelloIds.Tiles.D4)!).ToList();
        var d5Pieces = progress.State.GetPiecesOnTile(progress.Game.GetTile(OthelloIds.Tiles.D5)!).ToList();
        var e4Pieces = progress.State.GetPiecesOnTile(progress.Game.GetTile(OthelloIds.Tiles.E4)!).ToList();
        var e5Pieces = progress.State.GetPiecesOnTile(progress.Game.GetTile(OthelloIds.Tiles.E5)!).ToList();

        d4Pieces.Should().HaveCount(1);
        d5Pieces.Should().HaveCount(1);
        e4Pieces.Should().HaveCount(1);
        e5Pieces.Should().HaveCount(1);

        // d4 should be white, d5 should be black, e4 should be black, e5 should be white
        var d4Color = OthelloHelper.GetCurrentDiscColor(d4Pieces[0], progress.State);
        var d5Color = OthelloHelper.GetCurrentDiscColor(d5Pieces[0], progress.State);
        var e4Color = OthelloHelper.GetCurrentDiscColor(e4Pieces[0], progress.State);
        var e5Color = OthelloHelper.GetCurrentDiscColor(e5Pieces[0], progress.State);

        d4Color.Should().Be(OthelloDiscColor.White);
        d5Color.Should().Be(OthelloDiscColor.Black);
        e4Color.Should().Be(OthelloDiscColor.Black);
        e5Color.Should().Be(OthelloDiscColor.White);
    }

    [Fact]
    public void Should_have_black_as_active_player()
    {
        // arrange
        var builder = new OthelloGameBuilder();

        // act
        var progress = builder.Compile();

        // assert
        progress.State.TryGetActivePlayer(out var activePlayer).Should().BeTrue();
        activePlayer!.Id.Should().Be(OthelloIds.Players.Black);
    }
}
