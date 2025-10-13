using System.Linq;
using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;

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
        ok.Should().BeTrue();
        occupancy.PopCount().Should().Be(progress.State.GetStates<Boards.States.PieceState>().Count()); // all pieces occupy unique tiles
        perPlayer.Count.Should().Be(2);
        var white = perPlayer.Single(k => k.Key.Id == ChessIds.Players.White).Value.PopCount();
        var black = perPlayer.Single(k => k.Key.Id == ChessIds.Players.Black).Value.PopCount();
        (white + black).Should().Be(occupancy.PopCount());
    }
}