namespace Veggerby.Boards.Internal;

/// <summary>
/// Feature flags have been eliminated. This class remains as a compatibility shim for tests during migration.
/// All properties are no-ops and always return their graduated default values.
/// </summary>
/// <remarks>
/// DEPRECATED: This class will be removed in a future release. All features are now unconditionally enabled
/// based on their graduated status. Tests should be updated to remove FeatureFlagScope usage.
/// </remarks>
internal static class FeatureFlags
{
    // All graduated features - these were always true and are now unconditional in production code
    public static bool EnableCompiledPatterns { get; set; } = true;
    public static bool EnableBitboards { get; set; } = true;
    public static bool EnableStateHashing { get; set; } = true;
    public static bool EnableSlidingFastPath { get; set; } = true;
    public static bool EnableBitboardIncremental { get; set; } = true;
    public static bool EnableTurnSequencing { get; set; } = true;
    public static bool EnableTraceCapture { get; set; } = true;
    public static bool EnableBoardShape { get; set; } = true;

    // Removed experimental features - these are no-ops, always return false
    public static bool EnableTimelineZipper { get; set; } = false;
    public static bool EnableDecisionPlanGrouping { get; set; } = false;
    public static bool EnableDecisionPlanEventFiltering { get; set; } = false;
    public static bool EnableCompiledPatternsAdjacencyCache { get; set; } = false;
    public static bool EnableDecisionPlanMasks { get; set; } = false;
    public static bool EnablePerPieceMasks { get; set; } = false;
    public static bool EnableSegmentedBitboards { get; set; } = false;
    public static bool EnableTopologyPruning { get; set; } = false;
    public static bool EnableObserverBatching { get; set; } = false;
    public static bool EnableSimulation { get; set; } = true; // Simulation is always available via API
}
