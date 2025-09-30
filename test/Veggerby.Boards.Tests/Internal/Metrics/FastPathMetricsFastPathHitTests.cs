using Veggerby.Boards.Internal;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Internal.Metrics;

/// <summary>
/// Focused fast-path metrics coverage after parity restoration.
/// </summary>
public class FastPathMetricsFastPathHitTests
{
    private sealed class SimpleRookBoard : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "fastpath-rook";
            AddDirection(Constants.Directions.East);
            AddPlayer("white");
            AddTile("a1"); AddTile("b1"); AddTile("c1");
            WithTile("a1").WithRelationTo("b1").InDirection(Constants.Directions.East);
            WithTile("b1").WithRelationTo("c1").InDirection(Constants.Directions.East);
            AddPiece("rook").WithOwner("white").HasDirection(Constants.Directions.East).CanRepeat().OnTile("a1");
        }
    }

    [Fact]
    public void GivenSliderAndBitboardsEnabled_WhenResolving_ThenFastPathHitIncrements()
    {
        // arrange
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true); // sliding fast-path enabled by default
        var builder = new SimpleRookBoard();
        var progress = builder.Compile();
        var game = progress.Game;
        var rook = game.GetPiece("rook");
        var from = game.GetTile("a1");
        var to = game.GetTile("c1");

        // act
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        var snap = FastPathMetrics.Snapshot();

        // assert
        Assert.NotNull(path);
        Assert.Equal(1, snap.Attempts);
        Assert.True(snap.FastPathHits == 1 || snap.FastPathSkipAttackMiss >= 1, $"Expected fast-path hit or attack-miss skip, snapshot: hits={snap.FastPathHits}, miss={snap.FastPathSkipAttackMiss}");
        Assert.Equal(snap.Attempts, snap.FastPathHits + snap.CompiledHits + snap.LegacyHits + snap.FastPathSkipNoServices + snap.FastPathSkipNotSlider + snap.FastPathSkipAttackMiss + snap.FastPathSkipReconstructFail);
    }
}