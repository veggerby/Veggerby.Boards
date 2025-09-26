namespace Veggerby.Boards.Flows.Observers;

/// <summary>
/// Reason codes for rule skip notifications.
/// </summary>
public enum RuleSkipReason
{
    /// <summary>Skip due to event kind pre-filter (rule does not support current event kind).</summary>
    EventKindFiltered,
    /// <summary>Skip because another rule in the same exclusivity group already applied.</summary>
    ExclusivityMasked,
    /// <summary>Skip because group gate condition failed.</summary>
    GroupGateFailed
}