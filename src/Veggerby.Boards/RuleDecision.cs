using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards;

/// <summary>
/// Decision result capturing which rule was selected and why, with conflict-resolution details.
/// </summary>
/// <remarks>
/// Used for diagnostic logging and observability. Exposes the selected rule, rejected alternatives,
/// and a human-readable explanation of the resolution strategy outcome.
/// </remarks>
public sealed record RuleDecision
{
    /// <summary>
    /// Gets the event that was evaluated.
    /// </summary>
    public IGameEvent Event
    {
        get; init;
    }

    /// <summary>
    /// Gets the condition response from the selected rule.
    /// </summary>
    public ConditionResponse Response
    {
        get; init;
    }

    /// <summary>
    /// Gets metadata for the selected rule (null if no rule matched).
    /// </summary>
    public RuleMetadata? SelectedRule
    {
        get; init;
    }

    /// <summary>
    /// Gets metadata for rules that matched but were not selected due to conflict resolution.
    /// </summary>
    /// <remarks>
    /// Null when no conflict occurred. Non-empty list indicates multiple rules matched and conflict resolution was applied.
    /// </remarks>
    public IReadOnlyList<RuleMetadata>? RejectedRules
    {
        get; init;
    }

    /// <summary>
    /// Gets a human-readable explanation of why the selected rule won (priority comparison, first-wins, etc.).
    /// </summary>
    public string? SelectionReason
    {
        get; init;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RuleDecision"/>.
    /// </summary>
    /// <param name="Event">Event evaluated.</param>
    /// <param name="Response">Condition response from selected rule.</param>
    /// <param name="SelectedRule">Selected rule metadata.</param>
    /// <param name="RejectedRules">Rejected rule metadata.</param>
    /// <param name="SelectionReason">Explanation of selection.</param>
    public RuleDecision(
        IGameEvent Event,
        ConditionResponse Response,
        RuleMetadata? SelectedRule = null,
        IReadOnlyList<RuleMetadata>? RejectedRules = null,
        string? SelectionReason = null)
    {
        this.Event = Event;
        this.Response = Response;
        this.SelectedRule = SelectedRule;
        this.RejectedRules = RejectedRules;
        this.SelectionReason = SelectionReason;
    }
}
