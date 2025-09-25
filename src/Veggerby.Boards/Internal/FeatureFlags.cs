namespace Veggerby.Boards.Internal;

/// <summary>
/// Central runtime feature flags controlling activation of experimental engine components.
/// </summary>
/// <remarks>
/// All flags default to <c>false</c> unless a subsystem has graduated from experimental status via
/// comprehensive parity + safety tests. Compiled movement patterns have reached this bar and are now
/// enabled by default (visitor fallback retained on per-path miss). Remaining subsystems (DecisionPlan,
/// hashing, tracing, bitboards) remain opt-in until performance + determinism guards are finalized.
/// Flags are not serialized with game state to avoid hidden divergence.
/// </remarks>
internal static class FeatureFlags
{
    /// <summary>
    /// Gets or sets a value indicating whether the DecisionPlan executor replaces the legacy rule evaluation.
    /// Parity-only in initial implementation (no optimizations yet).
    /// </summary>
    public static bool EnableDecisionPlan { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether compiled movement patterns (DFA) are used instead of the visitor pattern.
    /// Default: true (parity suite + integration + adjacency cache parity validated).
    /// </summary>
    public static bool EnableCompiledPatterns { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether bitboard occupancy acceleration is enabled for boards with &lt;=64 tiles.
    /// Default: true (parity + benchmarks validated). Automatically skipped internally for larger boards (services not registered).
    /// </summary>
    public static bool EnableBitboards { get; set; } = false; // temporarily disabled pending occupancy incremental fix

    /// <summary>
    /// Gets or sets a value indicating whether state hashing (Merkle style) is performed each transition.
    /// (Placeholder – not yet implemented)
    /// </summary>
    public static bool EnableStateHashing { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether evaluation traces are captured (requires observer hooks).
    /// (Placeholder – not yet implemented)
    /// </summary>
    public static bool EnableTraceCapture { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the experimental timeline zipper (undo/redo structure) is active.
    /// </summary>
    public static bool EnableTimelineZipper { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether DecisionPlan grouping (gate predicate evaluated once per group) is enabled.
    /// When disabled, plan evaluation performs a simple linear scan of entries.
    /// </summary>
    public static bool EnableDecisionPlanGrouping { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether DecisionPlan event kind filtering is enabled (pre-filter entries by EventKind before predicate evaluation).
    /// (Future – scaffolding only)
    /// </summary>
    public static bool EnableDecisionPlanEventFiltering { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether DecisionPlan debug parity (shadow linear evaluation for verification) is enabled.
    /// (Future – scaffolding only)
    /// </summary>
    public static bool EnableDecisionPlanDebugParity { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the compiled pattern resolver should use a pre-built adjacency cache for (tile, direction) lookups.
    /// </summary>
    public static bool EnableCompiledPatternsAdjacencyCache { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether DecisionPlan exclusivity masks are enabled. When enabled, evaluation skips entries in the same
    /// exclusivity group once one entry in that group has successfully applied a rule for the current event.
    /// </summary>
    public static bool EnableDecisionPlanMasks { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the BoardShape adjacency layout fast-path is enabled for movement / pattern resolution.
    /// When enabled and a <see cref="Layout.BoardShape"/> service is registered, resolvers will prefer its O(1) neighbor lookup over
    /// scanning relations or using the compiled adjacency cache. This flag allows isolated benchmarking of BoardShape impact.
    /// Default: false (will be toggled after performance validation and parity tests).
    /// </summary>
    public static bool EnableBoardShape { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the sliding movement fast-path (bitboards + attack generator + path reconstruction)
    /// is enabled. Requires <see cref="EnableBitboards"/> and supporting services; when disabled, compiled/legacy paths are used exclusively.
    /// Default: true (comprehensive Parity V2 + benchmarks show >=4.6× speedup empty board, thresholds met). Can be toggled off for troubleshooting.
    /// </summary>
    public static bool EnableSlidingFastPath { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether per-piece occupancy masks are maintained (bitboard acceleration mode only).
    /// When enabled, an additional mask per piece id is computed to allow fine-grained attack pruning / mobility heuristics.
    /// Default: false (feature experimental; enable only when benchmarks confirm negligible overhead on move application).
    /// </summary>
    public static bool EnablePerPieceMasks { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether topology pruning heuristics are enabled (skip precomputation / lookup for
    /// directions not present in the board's <see cref="Internal.Layout.BoardTopology"/> classification). Intended to reduce
    /// branching and iteration in mixed topology boards. Default: false until pruning parity and performance are validated.
    /// </summary>
    public static bool EnableTopologyPruning { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the experimental simulation (sequential playout engine) features are enabled.
    /// When disabled, simulator APIs will throw <see cref="System.InvalidOperationException"/> if invoked to prevent
    /// accidental reliance on an experimental subsystem. Default: false until determinism + perf validated.
    /// </summary>
    public static bool EnableSimulation { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether evaluation observer callbacks are batched before dispatch to the configured
    /// <see cref="Flows.Observers.IEvaluationObserver"/>. When enabled, high-frequency callbacks (phase enter, rule evaluated, rule skipped)
    /// are buffered and flushed when a terminal outcome for the current event is reached (rule applied or event ignored) or the buffer capacity is exceeded.
    /// This reduces delegate invocation / virtual dispatch overhead in evaluation-intense scenarios (large plans) while preserving deterministic ordering.
    /// Default: false (opt-in until benchmarks validate &lt;=5% overhead vs direct dispatch for small plans).
    /// </summary>
    public static bool EnableObserverBatching { get; set; } = false;
}