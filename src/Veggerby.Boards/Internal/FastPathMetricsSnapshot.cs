namespace Veggerby.Boards.Internal;

internal readonly record struct FastPathMetricsSnapshot(long Attempts, long FastPathHits, long FastPathSkippedNoPrereq, long CompiledHits, long LegacyHits, long FastPathSkipNoServices, long FastPathSkipNotSlider, long FastPathSkipAttackMiss, long FastPathSkipReconstructFail)
{
    public long FastPathMisses => Attempts - FastPathHits;
}