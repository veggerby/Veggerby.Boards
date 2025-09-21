using System;

namespace Veggerby.Boards.Core.Internal;

/// <summary>
/// Central runtime feature flags controlling activation of upcoming experimental engine components.
/// </summary>
/// <remarks>
/// All flags default to <c>false</c>. They can be toggled in tests or hosting layers to gradually
/// introduce new subsystems (DecisionPlan, compiled patterns, hashing, tracing, bitboards) while
/// preserving deterministic behavior. Flags are not persisted across historical state and must be
/// treated strictly as environmental configuration.
/// </remarks>
internal static class FeatureFlags
{
    /// <summary>
    /// Gets or sets a value indicating whether the DecisionPlan executor replaces the legacy rule evaluation.
    /// </summary>
    public static bool EnableDecisionPlan { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether compiled movement patterns (DFA) are used instead of the visitor pattern.
    /// </summary>
    public static bool EnableCompiledPatterns { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether chess-specific bitboard acceleration is enabled.
    /// </summary>
    public static bool EnableBitboards { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether state hashing (Merkle style) is performed each transition.
    /// </summary>
    public static bool EnableStateHashing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether evaluation traces are captured (requires observer hooks).
    /// </summary>
    public static bool EnableTraceCapture { get; set; }
}
