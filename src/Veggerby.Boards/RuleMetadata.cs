namespace Veggerby.Boards;

/// <summary>
/// Metadata describing a rule for diagnostic and conflict-resolution purposes.
/// </summary>
/// <param name="RuleName">Human-readable rule identifier (typically type name or custom label).</param>
/// <param name="Priority">Assigned priority level.</param>
/// <param name="StrategyIdentifier">Optional domain-specific grouping hint (e.g., "castling", "en-passant").</param>
public sealed record RuleMetadata(
    string RuleName,
    RulePriority Priority,
    string? StrategyIdentifier = null);
