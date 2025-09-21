namespace Veggerby.Boards.Internal;

/// <summary>
/// Central runtime feature flags controlling activation of experimental engine components.
/// </summary>
/// <remarks>
/// All flags default to <c>false</c>. They can be toggled in tests or hosting layers to gradually
/// introduce new subsystems (DecisionPlan, compiled patterns, hashing, tracing, bitboards) while
/// preserving deterministic behavior. Flags are not serialized with game state to avoid hidden divergence.
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
    /// (Placeholder – not yet implemented)
    /// </summary>
    public static bool EnableCompiledPatterns { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether chess-specific bitboard acceleration is enabled.
    /// (Placeholder – not yet implemented)
    /// </summary>
    public static bool EnableBitboards { get; set; } = false;

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
}