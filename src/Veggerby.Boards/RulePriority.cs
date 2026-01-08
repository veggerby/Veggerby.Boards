namespace Veggerby.Boards;

/// <summary>
/// Assigns priority to rules for conflict resolution.
/// </summary>
/// <remarks>
/// Explicit priorities enable predictable rule ordering independent of declaration sequence.
/// Use sparingly—most rules should remain at <see cref="Normal"/> (50). Reserve <see cref="Override"/>
/// for explicit variant customizations. Same-priority ties resolve via declaration order.
/// </remarks>
public enum RulePriority
{
    /// <summary>
    /// Lowest priority (0). Evaluated last when multiple rules match.
    /// </summary>
    Lowest = 0,

    /// <summary>
    /// Low priority (25).
    /// </summary>
    Low = 25,

    /// <summary>
    /// Normal priority (50). Default for all rules when not explicitly specified.
    /// </summary>
    Normal = 50,

    /// <summary>
    /// High priority (75). Use for special-case rules that should preempt normal handling.
    /// </summary>
    High = 75,

    /// <summary>
    /// Highest priority (100). Use sparingly for critical precedence rules.
    /// </summary>
    Highest = 100,

    /// <summary>
    /// Override priority (200). Explicit variant override—use only when intentionally replacing base module behavior.
    /// </summary>
    Override = 200
}
