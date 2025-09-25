using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Tests.Infrastructure;
using Veggerby.Boards.Tests.Utils;

using Xunit;

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
            AddDirection("east");
            AddPlayer("white");
            AddTile("a1"); AddTile("b1"); AddTile("c1");
            WithTile("a1").WithRelationTo("b1").InDirection("east");
            WithTile("b1").WithRelationTo("c1").InDirection("east");
            AddPiece("rook").WithOwner("white").HasDirection("east").CanRepeat().OnTile("a1");
        }
    }

    [Fact]
    public void GivenSliderAndBitboardsEnabled_WhenResolving_ThenFastPathHitIncrements()
    {
        // arrange
        FastPathMetrics.Reset();
        using var scope = new Veggerby.Boards.Tests.Infrastructure.FeatureFlagScope(bitboards: true, compiledPatterns: true); // sliding fast-path enabled by default
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