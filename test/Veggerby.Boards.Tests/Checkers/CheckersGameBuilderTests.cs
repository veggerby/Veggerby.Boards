using Veggerby.Boards.Checkers;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Checkers;

public class CheckersGameBuilderTests
{
    private static Tuple<string, string> Direction(string direction, string tileId)
    {
        return new Tuple<string, string>(direction, tileId);
    }

    [Fact]
    public void Should_initialize_game_with_dark_square_topology()
    {
        // arrange & act
        var actual = new CheckersGameBuilder().Compile();

        // assert - verify dark square topology
        actual.Game.Board.Tiles.Should().HaveCount(32);
        
        // Verify some key diagonal connections
        actual.Game.ShouldHaveTileWithRelations("tile-1", 
            Direction("southeast", "tile-6"), 
            Direction("southwest", "tile-5"));
        
        actual.Game.ShouldHaveTileWithRelations("tile-9", 
            Direction("northeast", "tile-5"),
            Direction("northwest", "tile-6"),
            Direction("southeast", "tile-14"),
            Direction("southwest", "tile-13"));
    }

    [Fact]
    public void Should_place_black_pieces_on_tiles_1_to_12()
    {
        // arrange & act
        var actual = new CheckersGameBuilder().Compile();

        // assert
        actual.ShouldHavePieceState("black-piece-1", "tile-1");
        actual.ShouldHavePieceState("black-piece-2", "tile-2");
        actual.ShouldHavePieceState("black-piece-3", "tile-3");
        actual.ShouldHavePieceState("black-piece-4", "tile-4");
        actual.ShouldHavePieceState("black-piece-5", "tile-5");
        actual.ShouldHavePieceState("black-piece-6", "tile-6");
        actual.ShouldHavePieceState("black-piece-7", "tile-7");
        actual.ShouldHavePieceState("black-piece-8", "tile-8");
        actual.ShouldHavePieceState("black-piece-9", "tile-9");
        actual.ShouldHavePieceState("black-piece-10", "tile-10");
        actual.ShouldHavePieceState("black-piece-11", "tile-11");
        actual.ShouldHavePieceState("black-piece-12", "tile-12");
    }

    [Fact]
    public void Should_place_white_pieces_on_tiles_21_to_32()
    {
        // arrange & act
        var actual = new CheckersGameBuilder().Compile();

        // assert
        actual.ShouldHavePieceState("white-piece-1", "tile-21");
        actual.ShouldHavePieceState("white-piece-2", "tile-22");
        actual.ShouldHavePieceState("white-piece-3", "tile-23");
        actual.ShouldHavePieceState("white-piece-4", "tile-24");
        actual.ShouldHavePieceState("white-piece-5", "tile-25");
        actual.ShouldHavePieceState("white-piece-6", "tile-26");
        actual.ShouldHavePieceState("white-piece-7", "tile-27");
        actual.ShouldHavePieceState("white-piece-8", "tile-28");
        actual.ShouldHavePieceState("white-piece-9", "tile-29");
        actual.ShouldHavePieceState("white-piece-10", "tile-30");
        actual.ShouldHavePieceState("white-piece-11", "tile-31");
        actual.ShouldHavePieceState("white-piece-12", "tile-32");
    }

    [Fact]
    public void Should_set_black_as_active_player()
    {
        // arrange & act
        var actual = new CheckersGameBuilder().Compile();

        // assert
        var activePlayer = actual.State.TryGetActivePlayer(out var player);
        activePlayer.Should().BeTrue();
        player.Should().NotBeNull();
        player!.Id.Should().Be(CheckersIds.Players.Black);
    }
}
