using System.Linq;

using Veggerby.Boards.Go;
using Veggerby.Boards.Go.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Go;

public class GoBuilderSmokeTests
{
    [Fact]
    public void GivenNewGoGame_WhenBuilt_ThenBoardHasSize()
    {
        // arrange
        var builder = new GoGameBuilder(9);

        // act
        var progress = builder.Compile();
        var extras = progress.State.GetExtras<GoStateExtras>();

        // assert
        extras.BoardSize.Should().Be(9);
    }

    [Fact]
    public void GivenEmptyIntersection_WhenPlacingStone_ThenPieceStateAdded()
    {
        // arrange
        var builder = new GoGameBuilder(9);
        var progress = builder.Compile();
        var stone = progress.Game.GetPiece("black-stone-1");
        var target = progress.Game.GetTile("tile-1-1");
        var ev = new PlaceStoneGameEvent(stone, target);

        // act
        var updated = progress.HandleEvent(ev);

        // assert
        updated.State.GetStates<PieceState>().Any(ps => ps.Artifact == stone && ps.CurrentTile == target).Should().BeTrue();
    }
}