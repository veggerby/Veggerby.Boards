using System;
using System.Collections.Generic;
using System.Threading;

using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Tests.Infrastructure;

/// <summary>
/// Disposable helper for temporarily toggling internal feature flags inside tests.
/// Restores previous values on dispose to ensure isolation between tests.
/// <para>Thread-safety: Scopes are serialized across async contexts via a global semaphore. Nested scopes in the same async flow reuse the held lock and maintain a LIFO stack of snapshots for correct restoration.</para>
/// </summary>
public sealed class FeatureFlagScope : IDisposable
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly AsyncLocal<int> Depth = new();
    private static readonly AsyncLocal<Stack<(bool grouping, bool filtering, bool compiledPatterns, bool adjacencyCache, bool hashing, bool trace, bool timeline, bool masks, bool bitboards, bool boardShape, bool slidingFastPath)>> Snapshots = new();

    private bool _disposed;
    private readonly bool _isOuter;

    public FeatureFlagScope(
    bool? grouping = null,
    bool? filtering = null,
    bool? compiledPatterns = null,
    bool? adjacencyCache = null,
    bool? hashing = null,
    bool? trace = null,
    bool? timeline = null,
    bool? decisionPlanMasks = null,
    bool? bitboards = null,
    bool? boardShape = null,
    bool? slidingFastPath = null)
    {
        var depth = Depth.Value;
        if (depth == 0)
        {
            Gate.Wait();
            _isOuter = true;
            Snapshots.Value = new Stack<(bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool)>();
        }

        Snapshots.Value!.Push((
            FeatureFlags.EnableDecisionPlanGrouping,
            FeatureFlags.EnableDecisionPlanEventFiltering,
            FeatureFlags.EnableCompiledPatterns,
            FeatureFlags.EnableCompiledPatternsAdjacencyCache,
            FeatureFlags.EnableStateHashing,
            FeatureFlags.EnableTraceCapture,
            FeatureFlags.EnableTimelineZipper,
            FeatureFlags.EnableDecisionPlanMasks,
            FeatureFlags.EnableBitboards,
            FeatureFlags.EnableBoardShape,
            FeatureFlags.EnableSlidingFastPath));

        Depth.Value = depth + 1;

        if (grouping.HasValue)
        {
            FeatureFlags.EnableDecisionPlanGrouping = grouping.Value;
        }

        if (filtering.HasValue)
        {
            FeatureFlags.EnableDecisionPlanEventFiltering = filtering.Value;
        }

        if (compiledPatterns.HasValue)
        {
            FeatureFlags.EnableCompiledPatterns = compiledPatterns.Value;
        }

        if (adjacencyCache.HasValue)
        {
            FeatureFlags.EnableCompiledPatternsAdjacencyCache = adjacencyCache.Value;
        }

        if (hashing.HasValue)
        {
            FeatureFlags.EnableStateHashing = hashing.Value;
        }

        if (trace.HasValue)
        {
            FeatureFlags.EnableTraceCapture = trace.Value;
        }

        if (timeline.HasValue)
        {
            FeatureFlags.EnableTimelineZipper = timeline.Value;
        }

        if (decisionPlanMasks.HasValue)
        {
            FeatureFlags.EnableDecisionPlanMasks = decisionPlanMasks.Value;
        }

        if (bitboards.HasValue)
        {
            FeatureFlags.EnableBitboards = bitboards.Value;
        }

        if (boardShape.HasValue)
        {
            FeatureFlags.EnableBoardShape = boardShape.Value;
        }

        if (slidingFastPath.HasValue)
        {
            FeatureFlags.EnableSlidingFastPath = slidingFastPath.Value;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var (grouping, filtering, compiledPatterns, adjacencyCache, hashing, trace, timeline, masks, bitboards, boardShape, slidingFastPath) = Snapshots.Value!.Pop();
        FeatureFlags.EnableDecisionPlanGrouping = grouping;
        FeatureFlags.EnableDecisionPlanEventFiltering = filtering;
        FeatureFlags.EnableCompiledPatterns = compiledPatterns;
        FeatureFlags.EnableCompiledPatternsAdjacencyCache = adjacencyCache;
        FeatureFlags.EnableStateHashing = hashing;
        FeatureFlags.EnableTraceCapture = trace;
        FeatureFlags.EnableTimelineZipper = timeline;
        FeatureFlags.EnableDecisionPlanMasks = masks;
        FeatureFlags.EnableBitboards = bitboards;
        FeatureFlags.EnableBoardShape = boardShape;
        FeatureFlags.EnableSlidingFastPath = slidingFastPath;

        Depth.Value = Depth.Value - 1;
        if (_isOuter)
        {
            // Instead of assigning null (violates nullable reference expectations), clear stack to release references.
            Snapshots.Value!.Clear();
            Gate.Release();
        }
    }
}