using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal.Occupancy;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Utils;

using Xunit;

namespace Veggerby.Boards.Tests.Internal.Occupancy;

public class OccupancyIndexRegistrationTests
{
    private static (GameProgress progress, IOccupancyIndex occ) Build(bool bitboards)
    {
        using var scope = new FeatureFlagScope(bitboards: bitboards, compiledPatterns: true, boardShape: true);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var occ = progress.Engine.Capabilities?.Occupancy;
        occ.Should().NotBeNull();
        return (progress, occ);
    }

    [Fact]
    public void GivenBitboardsEnabled_WhenBuildingGame_ThenBitboardOccupancyIndexRegistered()
    {
        // arrange / act
        var (_, occ) = Build(bitboards: true);

        // assert
        occ.Should().BeOfType<BitboardOccupancyIndex>();
    }

    [Fact]
    public void GivenBitboardsDisabled_WhenBuildingGame_ThenNaiveOccupancyIndexRegistered()
    {
        // arrange / act
        var (_, occ) = Build(bitboards: false);

        // assert
        occ.Should().BeOfType<NaiveOccupancyIndex>();
    }

    [Fact]
    public void GivenBitboardsEnabled_WhenQueryingOccupancyIndex_ThenIsEmptyMatchesState()
    {
        var (progress, occ) = Build(bitboards: true);
        var tile = progress.Game.Board.Tiles.First(t => t.Id == "tile-a1");
        occ.IsEmpty(tile).Should().BeFalse();
    }

    [Fact]
    public void GivenNaiveOccupancyIndex_WhenQueryingOccupancy_ThenIsEmptyMatchesState()
    {
        var (progress, occ) = Build(bitboards: false);
        var tile = progress.Game.Board.Tiles.First(t => t.Id == "tile-a1");
        occ.IsEmpty(tile).Should().BeFalse();
    }
}