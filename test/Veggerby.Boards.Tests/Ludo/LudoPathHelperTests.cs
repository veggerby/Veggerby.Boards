using Veggerby.Boards.Ludo;

namespace Veggerby.Boards.Tests.Ludo;

public class LudoPathHelperTests
{
    [Fact]
    public void ResolveDestination_FromTrack0_MovesForward()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;
        var track0 = board.GetTile("track-0")!;

        // act - canEnterHome = false because piece hasn't completed a lap
        var destination = LudoPathHelper.ResolveDestination(board, track0, 3, "red", canEnterHome: false);

        // assert
        destination.Should().NotBeNull();
        destination!.Id.Should().Be("track-3");
    }

    [Fact]
    public void ResolveDestination_FromTrack0_MovesAroundBoard()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;
        var track50 = board.GetTile("track-50")!;

        // act - canEnterHome = false because piece hasn't completed a lap
        var destination = LudoPathHelper.ResolveDestination(board, track50, 4, "red", canEnterHome: false);

        // assert
        destination.Should().NotBeNull();
        destination!.Id.Should().Be("track-2"); // 50 + 4 wraps to 2 (52 tiles)
    }

    [Fact]
    public void ResolveDestination_AtHomeEntry_WithCanEnterHome_EntersHomeStretch()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;

        // Red's home entry is at track-0 (per LudoGameBuilder)
        var track0 = board.GetTile("track-0")!;

        // act - Red player at their home entry WITH canEnterHome=true should enter home stretch
        var destination = LudoPathHelper.ResolveDestination(board, track0, 2, "red", canEnterHome: true);

        // assert
        destination.Should().NotBeNull();
        destination!.Id.Should().Be("home-red-1"); // Enters home and moves 1 more step
    }

    [Fact]
    public void ResolveDestination_AtHomeEntry_WithoutCanEnterHome_ContinuesOnTrack()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;

        // Red's home entry is at track-0
        var track0 = board.GetTile("track-0")!;

        // act - Red player at home entry but canEnterHome=false should continue on track
        var destination = LudoPathHelper.ResolveDestination(board, track0, 2, "red", canEnterHome: false);

        // assert
        destination.Should().NotBeNull();
        destination!.Id.Should().Be("track-2"); // Continues on main track
    }

    [Fact]
    public void ResolveDestination_WrongPlayerAtHomeEntry_ContinuesOnTrack()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;

        // Red's home entry is at track-0
        // Blue at track-0 should NOT enter red's home even with canEnterHome=true
        var track0 = board.GetTile("track-0")!;

        // act - Blue player at red's home entry should continue on track
        var destination = LudoPathHelper.ResolveDestination(board, track0, 2, "blue", canEnterHome: true);

        // assert
        destination.Should().NotBeNull();
        destination!.Id.Should().Be("track-2"); // Continues on main track
    }

    [Fact]
    public void IsBaseTile_WithBaseTile_ReturnsTrue()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;
        var baseTile = board.GetTile("base-red")!;

        // act
        var result = LudoPathHelper.IsBaseTile(baseTile);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsBaseTile_WithTrackTile_ReturnsFalse()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;
        var trackTile = board.GetTile("track-5")!;

        // act
        var result = LudoPathHelper.IsBaseTile(trackTile);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsHomeTile_WithHomeTile_ReturnsTrue()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;
        var homeTile = board.GetTile("home-red-0")!;

        // act
        var result = LudoPathHelper.IsHomeTile(homeTile);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHomeTile_WithTrackTile_ReturnsFalse()
    {
        // arrange
        var builder = new LudoGameBuilder(playerCount: 2);
        var progress = builder.Compile();
        var board = progress.Engine.Game.Board;
        var trackTile = board.GetTile("track-5")!;

        // act
        var result = LudoPathHelper.IsHomeTile(trackTile);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetStartingTileId_ReturnsCorrectTileForEachColor()
    {
        // act & assert
        LudoPathHelper.GetStartingTileId("red").Should().Be("track-0");
        LudoPathHelper.GetStartingTileId("blue").Should().Be("track-13");
        LudoPathHelper.GetStartingTileId("green").Should().Be("track-26");
        LudoPathHelper.GetStartingTileId("yellow").Should().Be("track-39");
    }
}
