namespace Veggerby.Boards;

/// <summary>
/// Defines how conflicts between multiple matching rules are resolved.
/// </summary>
/// <remarks>
/// Strategies control which rule wins when multiple candidates match an event.
/// <see cref="FirstWins"/> preserves legacy declaration-order semantics (default).
/// <see cref="HighestPriority"/> enables explicit priority-based resolution.
/// <see cref="Exclusive"/> fails fast on conflicts (recommended during development).
/// </remarks>
public enum ConflictResolutionStrategy
{
    /// <summary>
    /// First matching rule wins (current behavior). Rules evaluated in declaration order.
    /// </summary>
    /// <remarks>
    /// Default strategy preserving backward compatibility. First valid rule encountered
    /// in the decision plan is selected; subsequent matches are ignored.
    /// </remarks>
    FirstWins,

    /// <summary>
    /// Highest priority rule wins. When multiple rules match, select the one with the highest <see cref="RulePriority"/>.
    /// </summary>
    /// <remarks>
    /// Ties (same priority) resolve via declaration order. Enables explicit precedence control
    /// without relying on implicit ordering.
    /// </remarks>
    HighestPriority,

    /// <summary>
    /// Last matching rule wins (override pattern). Enables layering where later declarations override earlier ones.
    /// </summary>
    /// <remarks>
    /// Useful for variant composition where derived modules override base rules by declaring
    /// replacements later in the phase tree.
    /// </remarks>
    LastWins,

    /// <summary>
    /// Throw exception if multiple rules match (fail-fast). Enforces explicit disambiguation.
    /// </summary>
    /// <remarks>
    /// Recommended during development to catch unintended rule overlaps. When multiple rules
    /// evaluate to Valid, throws <see cref="BoardException"/> listing conflicting rules.
    /// </remarks>
    Exclusive,

    /// <summary>
    /// Apply all matching rules in priority order (composition). Each rule mutates state sequentially.
    /// </summary>
    /// <remarks>
    /// Enables compositional effects (e.g., card game stacking). Rules applied highest priority
    /// first, with state threading through each mutation. Use cautiously—requires careful
    /// side-effect management to preserve determinism.
    /// </remarks>
    ApplyAll
}
