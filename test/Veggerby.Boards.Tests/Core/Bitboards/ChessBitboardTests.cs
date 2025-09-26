using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Bitboards;
using Veggerby.Boards.Tests.Infrastructure;
using Veggerby.Boards.Tests.Utils;

using Xunit;

namespace Veggerby.Boards.Tests.Core.Bitboards;

public class ChessBitboardTests
{
    [Fact]
    public void GivenInitialChessPosition_WhenBitboardsEnabled_ThenOccupancyMatchesPieceCount()
    {
        // arrange
        using var _ = new FeatureFlagScope(compiledPatterns: true, bitboards: true);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // act
        var ok = progress.TryGetBitboards(out var occupancy, out var perPlayer);

        // assert
        Assert.True(ok);
        Assert.Equal(progress.State.GetStates<Boards.States.PieceState>().Count(), occupancy.PopCount()); // all pieces occupy unique tiles
        Assert.Equal(2, perPlayer.Count);
        var white = perPlayer.Single(k => k.Key.Id == "white").Value.PopCount();
        var black = perPlayer.Single(k => k.Key.Id == "black").Value.PopCount();
        Assert.Equal(occupancy.PopCount(), white + black);
    }
}