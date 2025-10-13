using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Tests.Internal.Metrics;

public class FastPathMetricsInvariantTests
{
    [Fact]
    public void GivenSyntheticCounters_WhenSummed_ThenInvariantHolds()
    {
        // arrange
        FastPathMetrics.Reset();
        // Simulate attempts: 1 fast-path hit, 2 compiled hits, 1 legacy hit, and various skips
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnFastPathHit();
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnCompiledHit();
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnCompiledHit();
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnLegacyHit();
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnFastPathSkipNoServices();
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnFastPathSkipNotSlider();
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnFastPathSkipAttackMiss();
        FastPathMetrics.OnAttempt(); FastPathMetrics.OnFastPathSkipReconstructFail();

        // act
        var snap = FastPathMetrics.Snapshot();
        var attempts = snap.Attempts;
        var outcomeTotal = snap.FastPathHits + snap.CompiledHits + snap.LegacyHits + snap.FastPathSkipNoServices + snap.FastPathSkipNotSlider + snap.FastPathSkipAttackMiss + snap.FastPathSkipReconstructFail;

        // assert
        attempts.Should().Be(8);
        outcomeTotal.Should().Be(attempts);
    }
}