using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal.Occupancy;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Internal.Occupancy;

public class OccupancyIndexRegistrationTests
{
    private static (GameProgress progress, IOccupancyIndex occ) Build(bool bitboards)
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var occ = progress.Engine.Capabilities?.AccelerationContext.Occupancy;
        occ.Should().NotBeNull();
        return (progress, occ);
    }

    [Fact]
    public void GivenBitboardsEnabled_WhenBuildingGame_ThenBitboardOccupancyIndexRegistered()
    {
        // arrange

        // act

        // assert

        var (_, occ) = Build(bitboards: true);

        // assert
        occ.Should().BeOfType<BitboardOccupancyIndex>();
    }



    [Fact]
    public void GivenBitboardsEnabled_WhenQueryingOccupancyIndex_ThenIsEmptyMatchesState()
    {
        // arrange

        // act

        // assert

        var (progress, occ) = Build(bitboards: true);
        var tile = progress.Game.Board.Tiles.First(t => t.Id == Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A1);
        occ.IsEmpty(tile).Should().BeFalse();
    }

    [Fact]
    public void GivenNaiveOccupancyIndex_WhenQueryingOccupancy_ThenIsEmptyMatchesState()
    {
        // arrange

        // act

        // assert

        var (progress, occ) = Build(bitboards: false);
        var tile = progress.Game.Board.Tiles.First(t => t.Id == Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A1);
        occ.IsEmpty(tile).Should().BeFalse();
    }
}
