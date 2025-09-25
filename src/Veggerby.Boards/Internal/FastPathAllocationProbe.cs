using System;
using System.Runtime.CompilerServices;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Lightweight internal sampling probe for measuring GC collection deltas across a scoped block of sliding
/// fast-path resolutions. Intended for benchmark / diagnostic usage (not production hot path) to validate
/// allocation-free invariants for repeated empty-ray fast-path hits. Avoids embedding GC calls inside the
/// resolver itself (which would perturb measurements) by providing an opt-in scope.
/// </summary>
/// <remarks>
/// Usage pattern:
/// <code>
/// using var scope = FastPathAllocationProbe.Start();
/// // invoke paths repeatedly
/// var snapshot = scope.Stop();
/// </code>
/// The scope records GC.CollectionCount for generations 0..2 before and after. It does not force collections
/// or allocate per-iteration. This is deliberately coarse: if Gen0 collections remain unchanged after thousands
/// of fast-path hits in a benchmark harness, we treat the path as allocation-stable. Fine grained allocation
/// measurement should use BenchmarkDotNet's memory diagnoser.
/// </remarks>
internal readonly struct FastPathAllocationProbe
{
    private readonly int _gen0Before;
    private readonly int _gen1Before;
    private readonly int _gen2Before;
    private readonly long _ticksStart;
    private FastPathAllocationProbe(int g0, int g1, int g2, long ticks)
    {
        _gen0Before = g0; _gen1Before = g1; _gen2Before = g2; _ticksStart = ticks;
    }

    /// <summary>Begins a probe scope capturing initial GC collection counters.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FastPathAllocationProbe Start() => new(
        GC.CollectionCount(0),
        GC.CollectionCount(1),
        GC.CollectionCount(2),
        DateTime.UtcNow.Ticks);

    /// <summary>
    /// Completes the probe and returns a snapshot with delta metrics.
    /// </summary>
    public FastPathAllocationProbeSnapshot Stop()
    {
        var gen0After = GC.CollectionCount(0);
        var gen1After = GC.CollectionCount(1);
        var gen2After = GC.CollectionCount(2);
        return new FastPathAllocationProbeSnapshot(
            gen0After - _gen0Before,
            gen1After - _gen1Before,
            gen2After - _gen2Before,
            TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _ticksStart));
    }
}

/// <summary>
/// Result snapshot for a fast-path allocation probe (delta GC collections across generations 0..2 and elapsed time).
/// </summary>
/// <param name="Gen0CollectionsDelta">Difference in Gen0 collection count across the sampled window.</param>
/// <param name="Gen1CollectionsDelta">Difference in Gen1 collection count across the sampled window.</param>
/// <param name="Gen2CollectionsDelta">Difference in Gen2 collection count across the sampled window.</param>
/// <param name="Elapsed">Elapsed wall-clock duration for the probe scope.</param>
internal readonly record struct FastPathAllocationProbeSnapshot(int Gen0CollectionsDelta, int Gen1CollectionsDelta, int Gen2CollectionsDelta, TimeSpan Elapsed)
{
    /// <summary>Indicates whether no collections occurred in any generation.</summary>
    public bool IsZeroCollections => Gen0CollectionsDelta == 0 && Gen1CollectionsDelta == 0 && Gen2CollectionsDelta == 0;
}