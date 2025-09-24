using System.Threading;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Internal counters for sliding path resolution decision flow (fast-path vs compiled vs legacy).
/// Not exposed publicly; tests may access via reflection.
/// </summary>
internal static class FastPathMetrics
{
    private static long _attempts;
    private static long _fastPathHits;
    private static long _fastPathSkippedNoPrereq;
    private static long _compiledHits;
    private static long _legacyHits;

    public static void OnAttempt() => Interlocked.Increment(ref _attempts);
    public static void OnFastPathHit() => Interlocked.Increment(ref _fastPathHits);
    public static void OnFastPathSkippedNoPrereq() => Interlocked.Increment(ref _fastPathSkippedNoPrereq);
    public static void OnCompiledHit() => Interlocked.Increment(ref _compiledHits);
    public static void OnLegacyHit() => Interlocked.Increment(ref _legacyHits);

    public static FastPathMetricsSnapshot Snapshot() => new(
        Interlocked.Read(ref _attempts),
        Interlocked.Read(ref _fastPathHits),
        Interlocked.Read(ref _fastPathSkippedNoPrereq),
        Interlocked.Read(ref _compiledHits),
        Interlocked.Read(ref _legacyHits));

    public static void Reset()
    {
        Interlocked.Exchange(ref _attempts, 0);
        Interlocked.Exchange(ref _fastPathHits, 0);
        Interlocked.Exchange(ref _fastPathSkippedNoPrereq, 0);
        Interlocked.Exchange(ref _compiledHits, 0);
        Interlocked.Exchange(ref _legacyHits, 0);
    }
}

internal readonly record struct FastPathMetricsSnapshot(long Attempts, long FastPathHits, long FastPathSkippedNoPrereq, long CompiledHits, long LegacyHits)
{
    public long FastPathMisses => Attempts - FastPathHits;
}