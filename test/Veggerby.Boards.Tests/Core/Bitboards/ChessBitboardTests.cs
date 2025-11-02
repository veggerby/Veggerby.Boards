using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.Bitboards;

public class ChessBitboardTests
{
    [Fact]
    public void GivenInitialChessPosition_WhenBitboardsEnabled_ThenOccupancyMatchesPieceCount()
    {
        // arrange

        // act

        // assert

        using var _ = new FeatureFlagScope(compiledPatterns: true, bitboards: true);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // act
        var ok = progress.TryGetBitboards(out var occupancy, out var perPlayer);

        // assert
        ok.Should().BeTrue();
        occupancy.PopCount().Should().Be(progress.State.GetStates<Boards.States.PieceState>().Count()); // all pieces occupy unique tiles
        perPlayer.Should().NotBeNull();
        perPlayer!.Count.Should().Be(2);
        perPlayer.Should().NotBeNull();
        var whitePair = perPlayer.Single(k => k.Key.Id == ChessIds.Players.White);
        var blackPair = perPlayer.Single(k => k.Key.Id == ChessIds.Players.Black);
        whitePair.Value.Should().NotBeNull();
        blackPair.Value.Should().NotBeNull();
        var white = whitePair.Value.PopCount();
        var black = blackPair.Value.PopCount();
        (white + black).Should().Be(occupancy.PopCount());
    }
}
