using System;
using System.Linq;

using Veggerby.Boards.Othello;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Othello;

public class OthelloDiscPlacementTests
{
    [Fact]
    public void Should_place_disc_and_flip_opponent_disc()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        var disc = progress.Game.GetPiece("black-disc-3")!;
        var tile = progress.Game.GetTile(OthelloIds.Tiles.D3)!;
        var placeEvent = new PlaceDiscGameEvent(disc, tile);

        // act
        progress = progress.HandleEvent(placeEvent);

        // assert
        // d3 should now have a black disc
        var d3Pieces = progress.State.GetPiecesOnTile(tile).ToList();
        d3Pieces.Should().HaveCount(1);

        // d4 should have been flipped from white to black
        var d4Tile = progress.Game.GetTile(OthelloIds.Tiles.D4)!;
        var d4Pieces = progress.State.GetPiecesOnTile(d4Tile).ToList();
        d4Pieces.Should().HaveCount(1);
        var d4Color = OthelloHelper.GetCurrentDiscColor(d4Pieces[0], progress.State);
        d4Color.Should().Be(OthelloDiscColor.Black);

        // Should now have 4 black discs and 1 white disc
        var blackCount = progress.State.GetStates<PieceState>()
            .Count(ps => OthelloHelper.GetCurrentDiscColor(ps.Artifact, progress.State) == OthelloDiscColor.Black);
        var whiteCount = progress.State.GetStates<PieceState>()
            .Count(ps => OthelloHelper.GetCurrentDiscColor(ps.Artifact, progress.State) == OthelloDiscColor.White);

        blackCount.Should().Be(4);
        whiteCount.Should().Be(1);
    }

    [Fact]
    public void Should_reject_placement_on_occupied_tile()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        var disc = progress.Game.GetPiece("black-disc-3")!;
        var tile = progress.Game.GetTile(OthelloIds.Tiles.D4)!; // Already occupied by white
        var placeEvent = new PlaceDiscGameEvent(disc, tile);

        // act & assert
        Action act = () => progress.HandleEvent(placeEvent);
        act.Should().Throw<InvalidGameEventException>().WithMessage("*already occupied*");
    }

    [Fact]
    public void Should_reject_placement_that_does_not_flip_any_disc()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        var disc = progress.Game.GetPiece("black-disc-3")!;
        var tile = progress.Game.GetTile(OthelloIds.Tiles.A1)!; // Corner, no adjacent opponent discs to flip
        var placeEvent = new PlaceDiscGameEvent(disc, tile);

        // act & assert
        Action act = () => progress.HandleEvent(placeEvent);
        act.Should().Throw<InvalidGameEventException>().WithMessage("*must flip at least one opponent disc*");
    }

    [Fact]
    public void Should_switch_to_next_player_after_placement()
    {
        // arrange
        var builder = new OthelloGameBuilder();
        var progress = builder.Compile();

        progress.State.TryGetActivePlayer(out var initialPlayer).Should().BeTrue();
        initialPlayer!.Id.Should().Be(OthelloIds.Players.Black);

        var disc = progress.Game.GetPiece("black-disc-3")!;
        var tile = progress.Game.GetTile(OthelloIds.Tiles.D3)!;
        var placeEvent = new PlaceDiscGameEvent(disc, tile);

        // act
        progress = progress.HandleEvent(placeEvent);

        // assert
        progress.State.TryGetActivePlayer(out var nextPlayer).Should().BeTrue();
        nextPlayer!.Id.Should().Be(OthelloIds.Players.White);
    }
}
