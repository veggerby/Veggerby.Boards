using System;

using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Flows.Observers;

/// <summary>
/// Observer interface for instrumentation of rule evaluation and application within a game event handling cycle.
/// </summary>
/// <remarks>
/// Implementations MUST be side-effect free with respect to game state mutation. They may record metrics, traces
/// or diagnostics externally but must never alter <see cref="GameState"/>. All callbacks are optional; the engine
/// guarantees they are invoked in deterministic order for identical inputs. Avoid heavy allocations.
/// </remarks>
public interface IEvaluationObserver
{
    /// <summary>
    /// Called when evaluation for an event enters (considers) a phase before any rule checks occur.
    /// </summary>
    /// <param name="phase">Phase being entered.</param>
    /// <param name="state">State at time of entry.</param>
    void OnPhaseEnter(GamePhase phase, GameState state);
    /// <summary>
    /// Called after a rule's condition has been evaluated.
    /// </summary>
    /// <param name="phase">Active phase containing the rule.</param>
    /// <param name="rule">The evaluated rule.</param>
    /// <param name="response">Result of the condition evaluation.</param>
    /// <param name="state">The game state at evaluation time.</param>
    void OnRuleEvaluated(GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state);

    /// <summary>
    /// Called after a rule has been applied successfully producing a new game state.
    /// </summary>
    /// <param name="phase">Phase in which the rule was applied.</param>
    /// <param name="rule">Applied rule.</param>
    /// <param name="event">The triggering event.</param>
    /// <param name="beforeState">State prior to application.</param>
    /// <param name="afterState">Resulting state after application.</param>
    void OnRuleApplied(GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState);

    /// <summary>
    /// Called when an event was ignored (no applicable rules in any active phase).
    /// </summary>
    /// <param name="event">Ignored event.</param>
    /// <param name="state">State at the time of ignoring.</param>
    void OnEventIgnored(IGameEvent @event, GameState state);
}

/// <summary>
/// No-op observer implementation avoiding null checks; used when no instrumentation is required.
/// </summary>
public sealed class NullEvaluationObserver : IEvaluationObserver
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly IEvaluationObserver Instance = new NullEvaluationObserver();

    private NullEvaluationObserver()
    {
    }

    /// <inheritdoc />
    public void OnRuleEvaluated(GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state)
    {
    }

    /// <inheritdoc />
    public void OnRuleApplied(GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState)
    {
    }

    /// <inheritdoc />
    public void OnEventIgnored(IGameEvent @event, GameState state)
    {
    }

    /// <inheritdoc />
    public void OnPhaseEnter(GamePhase phase, GameState state)
    {
    }
}