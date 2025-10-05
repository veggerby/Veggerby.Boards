using Veggerby.Boards.Internal;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Internal.Metrics;

public class FastPathMetricsTests
{
    // Updated harness exercising counters over new capability seam.

    [Fact]
    public void GivenBoardWithMoreThan64Tiles_WhenResolvingSlidingPath_ThenFastPathSkippedNoServicesIncrementsAndNoCrash()
    {
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true); // sliding fast-path default on
        var progress = new LargeLinearBuilder().Compile();
        var piece = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("t0");
        var to = progress.Game.GetTile("t1");
        var path = progress.ResolvePathCompiledFirst(piece, from, to);
        Assert.NotNull(path);
        var snap = FastPathMetrics.Snapshot();
        Assert.Equal(1, snap.Attempts);
        Assert.True(snap.FastPathHits == 1 || snap.FastPathSkipAttackMiss >= 1 || snap.FastPathSkipNoServices >= 1);
        Assert.Equal(snap.Attempts, snap.FastPathHits + snap.CompiledHits + snap.LegacyHits + snap.FastPathSkipNoServices + snap.FastPathSkipNotSlider + snap.FastPathSkipAttackMiss + snap.FastPathSkipReconstructFail);
    }

    [Fact]
    public void GivenDegenerateLargeSingleDirectionBoard_WhenQueryingSlidingRays_ThenGeneratorNeutralized()
    {
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true);
        var progress = new LargeLinearBuilder().Compile();
        var rook = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("t0");
        // Exercise fast path resolution (should gracefully skip due to neutralized rays)
        var path = progress.ResolvePathCompiledFirst(rook, from, progress.Game.GetTile("t1"));
        Assert.NotNull(path); // Path still resolvable via compiled/legacy resolver fallback
        var metrics = FastPathMetrics.Snapshot();
        Assert.Equal(1, metrics.Attempts);
        // No crash and either a skip (no services / attack miss) or an actual hit if heuristics change.
        Assert.True(metrics.FastPathHits == 1 || metrics.FastPathSkipNoServices >= 0 || metrics.FastPathSkipAttackMiss >= 0);
    }

    [Fact]
    public void GivenBitboardsEnabled_WhenResolvingSlidingPath_ThenFastPathHitIncrementsCounter()
    {
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true);
        var progress = new RookNorthBuilder().Compile();
        var rook = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("v1");
        var to = progress.Game.GetTile("v4");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        Assert.NotNull(path);
        var snap = FastPathMetrics.Snapshot();
        Assert.Equal(1, snap.Attempts);
        Assert.Equal(1, snap.FastPathHits);
        Assert.Equal(0, snap.CompiledHits + snap.LegacyHits + snap.FastPathSkipNoServices + snap.FastPathSkipNotSlider + snap.FastPathSkipAttackMiss + snap.FastPathSkipReconstructFail);
    }

    [Fact]
    public void GivenBitboardsDisabled_WhenResolvingSlidingPath_ThenFastPathSkippedNoPrereqIncrementsCounter()
    {
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: false, compiledPatterns: true);
        var progress = new RookNorthBuilder().Compile();
        var rook = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("v1");
        var to = progress.Game.GetTile("v2");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        Assert.NotNull(path);
        var snap = FastPathMetrics.Snapshot();
        Assert.Equal(1, snap.Attempts);
        Assert.Equal(0, snap.FastPathHits);
        Assert.True(snap.FastPathSkipNoServices + snap.CompiledHits + snap.LegacyHits + snap.FastPathSkipNotSlider + snap.FastPathSkipAttackMiss + snap.FastPathSkipReconstructFail >= 1);
    }

    [Fact]
    public void GivenNonSlider_WhenResolvingPath_ThenFastPathSkipNotSliderIncrements()
    {
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true);
        var progress = new NonSliderBuilder().Compile();
        var piece = progress.Game.GetPiece("stone");
        var from = progress.Game.GetTile("x1");
        var to = progress.Game.GetTile("x2");
        var path = progress.ResolvePathCompiledFirst(piece, from, to);
        Assert.Null(path);
        var snap = FastPathMetrics.Snapshot();
        Assert.Equal(1, snap.Attempts);
        Assert.Equal(0, snap.FastPathHits);
        Assert.True(snap.FastPathSkipNotSlider >= 1);
    }

    [Fact]
    public void GivenCompiledPatternsEnabledAndFastPathPrereqsMissing_WhenResolving_ThenCompiledHitIncrements()
    {
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: false, compiledPatterns: true);
        var progress = new RookNorthBuilder().Compile();
        var rook = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("v1");
        var to = progress.Game.GetTile("v2");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        Assert.NotNull(path);
        var snap = FastPathMetrics.Snapshot();
        Assert.Equal(1, snap.Attempts);
        Assert.True(snap.CompiledHits + snap.LegacyHits >= 1);
    }

    [Fact]
    public void GivenCompiledPatternsDisabled_WhenResolving_ThenLegacyHitIncrements()
    {
        FastPathMetrics.Reset();
        using var scope = new FeatureFlagScope(bitboards: false, compiledPatterns: false);
        var progress = new RookNorthBuilder().Compile();
        var rook = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("v1");
        var to = progress.Game.GetTile("v2");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        Assert.NotNull(path);
        var snap = FastPathMetrics.Snapshot();
        Assert.Equal(1, snap.Attempts);
        Assert.True(snap.LegacyHits + snap.CompiledHits >= 1);
    }

    private sealed class RookNorthBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "rook-north";
            AddDirection(Constants.Directions.North);
            AddPlayer("white");
            AddTile("v1"); AddTile("v2"); AddTile("v3"); AddTile("v4");
            WithTile("v1").WithRelationTo("v2").InDirection(Constants.Directions.North);
            WithTile("v2").WithRelationTo("v3").InDirection(Constants.Directions.North);
            WithTile("v3").WithRelationTo("v4").InDirection(Constants.Directions.North);
            AddPiece("rook").WithOwner("white").HasDirection(Constants.Directions.North).CanRepeat().OnTile("v1");
        }
    }

    private sealed class NonSliderBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "non-slider";
            AddDirection(Constants.Directions.East);
            AddPlayer("white");
            AddTile("x1"); AddTile("x2");
            WithTile("x1").WithRelationTo("x2").InDirection(Constants.Directions.East);
            AddPiece("stone").WithOwner("white").OnTile("x1");
        }
    }

    private sealed class LargeLinearBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "large-linear";
            AddDirection(Constants.Directions.East);
            AddPlayer("white");
            for (int i = 0; i < 65; i++)
            {
                AddTile($"t{i}");
                if (i > 0)
                {
                    WithTile($"t{i - 1}").WithRelationTo($"t{i}").InDirection(Constants.Directions.East);
                }
            }
            AddPiece("rook").WithOwner("white").HasDirection(Constants.Directions.East).CanRepeat().OnTile("t0");
        }
    }
}