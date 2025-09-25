using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Simulation;

/// <summary>
/// Aggregated result for executing multiple playouts.
/// </summary>
/// <param name="Results">Individual playout results.</param>
public sealed record PlayoutBatchResult(IReadOnlyList<PlayoutResult> Results)
{
    /// <summary>Total number of playouts executed.</summary>
    public int Count => Results.Count;

    /// <summary>Number of playouts where at least one event applied.</summary>
    public int ProgressedCount => Results.Count(r => r.Progressed);

    /// <summary>Total applied events across all playouts.</summary>
    public int TotalApplied => Results.Sum(r => r.AppliedEvents);

    private int[] _histogram;

    /// <summary>
    /// Lazy-computed histogram where index = applied event count and value = number of playouts with that length.
    /// </summary>
    public IReadOnlyList<int> Histogram
    {
        get
        {
            if (_histogram is null)
            {
                if (Results.Count == 0)
                {
                    _histogram = System.Array.Empty<int>();
                }
                else
                {
                    var max = Results.Max(r => r.AppliedEvents);
                    var arr = new int[max + 1];

                    foreach (var r in Results)
                    {
                        arr[r.AppliedEvents]++;
                    }

                    _histogram = arr;
                }
            }

            return _histogram;
        }
    }

    /// <summary>Minimum applied event count across playouts (0 when no results).</summary>
    public int MinApplied => Results.Count == 0 ? 0 : Results.Min(r => r.AppliedEvents);

    /// <summary>Maximum applied event count across playouts (0 when no results).</summary>
    public int MaxApplied => Results.Count == 0 ? 0 : Results.Max(r => r.AppliedEvents);

    /// <summary>Average applied events (floating) across playouts; 0 when none.</summary>
    public double AverageApplied => Results.Count == 0 ? 0d : (double)TotalApplied / Results.Count;

    private double? _variance;

    /// <summary>Population variance of applied event counts (0 when fewer than 2 results).</summary>
    public double Variance
    {
        get
        {
            if (_variance.HasValue)
            {
                return _variance.Value;
            }
            if (Results.Count < 2)
            {
                _variance = 0d;
            }
            else
            {
                var mean = AverageApplied;
                double sumSq = 0;
                foreach (var r in Results)
                {
                    var diff = r.AppliedEvents - mean;
                    sumSq += diff * diff;
                }
                _variance = sumSq / Results.Count; // population variance
            }
            return _variance.Value;
        }
    }

    /// <summary>Population standard deviation (sqrt of variance).</summary>
    public double StandardDeviation => System.Math.Sqrt(Variance);

    /// <summary>
    /// Returns the P-th percentile (0-100) of applied event counts using nearest-rank method.
    /// Returns 0 if no results. Clamps p into [0,100].
    /// </summary>
    public int Percentile(double p)
    {
        if (Results.Count == 0)
        {
            return 0;
        }
        if (p <= 0) { return MinApplied; }
        if (p >= 100) { return MaxApplied; }
        var ordered = Results.Select(r => r.AppliedEvents).OrderBy(v => v).ToArray();
        var rank = (int)System.Math.Ceiling((p / 100d) * ordered.Length);
        var index = System.Math.Max(1, rank) - 1;
        return ordered[index];
    }
}

/// <summary>
/// Aggregated metrics for a single playout (captured only in detailed runs to avoid overhead otherwise).
/// </summary>
/// <remarks>
/// Creates a new metrics snapshot.
/// </remarks>
/// <param name="applied">Applied event count.</param>
/// <param name="rejected">Rejected candidate event count.</param>
/// <param name="policyCalls">Number of policy invocations.</param>
/// <param name="maxDepthObserved">Maximum depth reached (events applied).</param>
public sealed class PlayoutMetrics(int applied, int rejected, int policyCalls, int maxDepthObserved)
{
    /// <summary>
    /// Number of events successfully applied during the playout.
    /// </summary>
    public int AppliedEvents { get; } = applied;

    /// <summary>
    /// Number of candidate events rejected (either by rules or policy). May be zero if the policy does not expose rejected attempts.
    /// </summary>
    public int RejectedEvents { get; } = rejected;

    /// <summary>
    /// Number of times the policy delegate was invoked to obtain the next event.
    /// </summary>
    public int PolicyCalls { get; } = policyCalls;

    /// <summary>
    /// Maximum depth (number of applied events) observed at any point in the playout. Equal to <see cref="AppliedEvents"/> unless early termination semantics evolve.
    /// </summary>
    public int MaxDepthObserved { get; } = maxDepthObserved;
}

/// <summary>
/// Detailed batch result including per-playout metrics and aggregate summaries.
/// </summary>
/// <remarks>
/// Creates a new detailed batch result.
/// </remarks>
public sealed class PlayoutBatchDetailedResult(PlayoutBatchResult basic, IReadOnlyList<PlayoutMetrics> metrics, bool cancellationRequested)
{
    /// <summary>
    /// Basic batch result (playout results and simple statistics) that mirrors the non-detailed API for compatibility.
    /// </summary>
    public PlayoutBatchResult Basic { get; } = basic;

    /// <summary>
    /// Per-playout metrics captured during detailed execution.
    /// </summary>
    public IReadOnlyList<PlayoutMetrics> Metrics { get; } = metrics;

    /// <summary>
    /// Indicates whether cancellation was requested mid-run and the batch is therefore partial.
    /// </summary>
    public bool CancellationRequested { get; } = cancellationRequested;

    /// <summary>
    /// Total applied events across all playouts in this batch.
    /// </summary>
    public int TotalApplied => Metrics.Sum(m => m.AppliedEvents);

    /// <summary>
    /// Total rejected candidate events across all playouts.
    /// </summary>
    public int TotalRejected => Metrics.Sum(m => m.RejectedEvents);

    /// <summary>
    /// Total policy invocations across all playouts.
    /// </summary>
    public int TotalPolicyCalls => Metrics.Sum(m => m.PolicyCalls);

    /// <summary>
    /// Average branching factor approximation = total policy calls / playout count (coarse; refined variant may divide by applied depth sum later).
    /// </summary>
    public double AverageBranchingFactor => Metrics.Count == 0 ? 0d : (double)TotalPolicyCalls / Metrics.Count;
}