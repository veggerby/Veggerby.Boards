using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Observers;

namespace Veggerby.Boards.States;

/// <summary>
/// Represents a snapshot of game evolution including the current <see cref="GameState"/>, active phase,
/// accumulated events and engine context.
/// </summary>
/// <remarks>
/// <see cref="GameProgress"/> is immutable; handling an event produces a new instance. This enables
/// functional style evaluation and potential branching for analysis or AI without mutating prior history.
/// </remarks>
public class GameProgress
{
    /// <summary>
    /// Initializes a new <see cref="GameProgress"/> instance.
    /// </summary>
    /// <param name="engine">The game engine hosting rules and phase graph.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="events">The historical events (optional).</param>
    public GameProgress(GameEngine engine, GameState state, IEnumerable<IGameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(engine);

        ArgumentNullException.ThrowIfNull(state);

        Engine = engine;
        State = state;
        Events = [.. (events ?? Enumerable.Empty<IGameEvent>())];
        Phase = Engine.GamePhaseRoot.GetActiveGamePhase(State);
    }

    /// <summary>
    /// Gets the engine context.
    /// </summary>
    public GameEngine Engine { get; }

    /// <summary>
    /// Gets the current state snapshot.
    /// </summary>
    public GameState State { get; }

    /// <summary>
    /// Gets the active phase derived from the phase tree and current state.
    /// </summary>
    public GamePhase Phase { get; }

    /// <summary>
    /// Gets the event history up to this progress state.
    /// </summary>
    public IEnumerable<IGameEvent> Events { get; }

    /// <summary>
    /// Gets the root game definition.
    /// </summary>
    public Game Game => Engine.Game;

    private GameProgress HandleSingleEvent(IGameEvent @event)
    {
        // Notify phase entry (legacy path active phase only)
        Engine.Observer.OnPhaseEnter(Phase, State);
        var ruleCheck = Phase.Rule.Check(Engine, State, @event);
    Engine.Observer.OnRuleEvaluated(Phase, Phase.Rule, ruleCheck, State);
        if (ruleCheck.Result == ConditionResult.Valid)
        {
            var newState = Phase.Rule.HandleEvent(Engine, State, @event);
            Engine.Observer.OnRuleApplied(Phase, Phase.Rule, @event, State, newState);
            return new GameProgress(Engine, newState, Events.Concat([@event]));
        }
        else if (ruleCheck.Result == ConditionResult.Invalid)
        {
            throw new InvalidGameEventException(@event, ruleCheck, Game, Phase, State);
        }

        // ignored
        Engine.Observer.OnEventIgnored(@event, State);
        return this;
    }

    /// <summary>
    /// Produces a new <see cref="GameProgress"/> with a successor <see cref="GameState"/> built from replacing the provided artifact states.
    /// </summary>
    /// <param name="newStates">The new artifact states to apply.</param>
    /// <returns>A new progress instance.</returns>
    public GameProgress NewState(IEnumerable<IArtifactState> newStates)
    {
        return new GameProgress(
            Engine,
            State.Next(newStates),
            Events
        );
    }

    /// <summary>
    /// Handles a game event (with pre-processing) producing a new immutable progress instance.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>The resulting progress.</returns>
    public GameProgress HandleEvent(IGameEvent @event)
    {
        // Legacy path retains phase resolution per event when DecisionPlan feature flag is disabled or plan absent.
        if (Engine.DecisionPlan is null || !Internal.FeatureFlags.EnableDecisionPlan)
        {
            var events = Phase.PreProcessEvent(this, @event);
            return events.Aggregate(this, (seed, e) => seed.HandleSingleEvent(e));
        }

        // DecisionPlan parity path: iterate precompiled leaf phases in order, selecting first whose condition is valid.
        // Pre-processing still uses the currently active phase determined at construction (could be refined later
        // to pre-process per candidate phase if needed). This maintains functional parity with legacy traversal.
        var preProcessed = Phase.PreProcessEvent(this, @event);
        var progress = this;
        foreach (var evt in preProcessed)
        {
            var handled = false;
            foreach (var entry in Engine.DecisionPlan.Entries)
            {
                if (!entry.Condition.Evaluate(progress.State).Equals(ConditionResponse.Valid))
                {
                    continue;
                }

                var ruleCheck = entry.Rule.Check(progress.Engine, progress.State, evt);
                    // Resolve phase by number for accurate observer attribution (depth-first traversal cost acceptable for V1).
                    var observedPhase = Flows.DecisionPlan.DecisionPlan.ResolvePhase(progress.Engine.GamePhaseRoot, entry.PhaseNumber) ?? progress.Engine.GamePhaseRoot;
                    progress.Engine.Observer.OnPhaseEnter(observedPhase, progress.State);
                    progress.Engine.Observer.OnRuleEvaluated(observedPhase, entry.Rule, ruleCheck, progress.State);
                if (ruleCheck.Result == ConditionResult.Valid)
                {
                    var newState = entry.Rule.HandleEvent(progress.Engine, progress.State, evt);
                    progress.Engine.Observer.OnRuleApplied(observedPhase, entry.Rule, evt, progress.State, newState);
                    progress = new GameProgress(progress.Engine, newState, progress.Events.Concat([evt]));
                    handled = true;
                    break; // only first valid rule/phase handles event (parity with existing behavior)
                }
                else if (ruleCheck.Result == ConditionResult.Invalid)
                {
                    throw new InvalidGameEventException(evt, ruleCheck, progress.Game, progress.Phase, progress.State);
                }
            }

            if (!handled)
            {
                // Event ignored (no applicable phase/rule).
                progress.Engine.Observer.OnEventIgnored(evt, progress.State);
            }
        }
        return progress;
    }
}