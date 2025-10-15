using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Events;

/// <summary>
/// Reasons describing why an event did not apply (when <see cref="EventResult.Applied"/> is false).
/// </summary>
public enum EventRejectionReason
{
    /// <summary>No rejection (successful application).</summary>
    None = 0,
    /// <summary>The active phase does not accept this event.</summary>
    PhaseClosed = 1,
    /// <summary>No rule accepted the event (ignored or not applicable).</summary>
    NotApplicable = 2,
    /// <summary>Actor attempting the event does not own the target piece or artifact.</summary>
    InvalidOwnership = 3,
    /// <summary>No movement/path pattern could resolve a path required for the event.</summary>
    PathNotFound = 4,
    /// <summary>A rule explicitly rejected the event (ConditionResult.Invalid).</summary>
    RuleRejected = 5,
    /// <summary>Malformed or semantically invalid event payload.</summary>
    InvalidEvent = 6,
    /// <summary>Internal engine invariant broken (should surface for diagnostics).</summary>
    EngineInvariant = 7,
}

/// <summary>
/// Represents the outcome of attempting to handle a game event including the resulting state (original if rejected),
/// whether it was applied, and a structured rejection reason.
/// </summary>
/// <remarks>
/// Stable deterministic mapping: identical input state + event always yields identical <see cref="EventResult"/>.
/// The legacy <c>HandleEvent</c> method may still throw for catastrophic invariants; this result focuses on
/// expected domain-level rejections. The extension method wrapper is obsolete; prefer the instance API.
/// </remarks>
/// <param name="State">The resulting (or original) state snapshot.</param>
/// <param name="Applied">True if a rule applied and produced a successor state.</param>
/// <param name="Reason">Rejection reason when not applied.</param>
/// <param name="Message">Optional human-readable detail.</param>
public readonly record struct EventResult(GameState State, bool Applied, EventRejectionReason Reason, string? Message)
{
    /// <summary>
    /// Creates a successful accepted result.
    /// </summary>
    public static EventResult Accepted(GameState state) => new(state, true, EventRejectionReason.None, null);

    /// <summary>
    /// Creates a rejected result with the specified reason and optional message.
    /// </summary>
    public static EventResult Rejected(GameState state, EventRejectionReason reason, string? message = null) => new(state, false, reason, message);

    /// <summary>
    /// Convenience to project to resulting state (original if rejected) for transitional call sites.
    /// </summary>
    public GameState EffectiveState => State;
}