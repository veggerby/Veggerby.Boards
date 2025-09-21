using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;

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
        var debugParityEnabled = Internal.FeatureFlags.EnableDecisionPlan && Internal.FeatureFlags.EnableDecisionPlanDebugParity;
        var originalProgress = this; // capture starting snapshot for shadow evaluation
        // Legacy path retains phase resolution per event when DecisionPlan feature flag is disabled or plan absent.
        if (Engine.DecisionPlan is null || !Internal.FeatureFlags.EnableDecisionPlan)
        {
            return HandleEventLegacy(@event);
        }

        // DecisionPlan parity path: iterate precompiled leaf phases in order, selecting first whose condition is valid.
        // Pre-processing still uses the currently active phase determined at construction (could be refined later
        // to pre-process per candidate phase if needed). This maintains functional parity with legacy traversal.
        // Phase can be null if no active phase resolves for current state; in that case, skip pre-processing.
        var preProcessed = Phase is null ? new[] { @event } : Phase.PreProcessEvent(this, @event);
        var progress = this;
        foreach (var evt in preProcessed)
        {
            var handled = false;
            var eventKindFiltering = Internal.FeatureFlags.EnableDecisionPlanEventFiltering;
            var currentEventKind = eventKindFiltering ? EventKindClassifier.Classify(evt) : EventKind.Any;
            var masksEnabled = Internal.FeatureFlags.EnableDecisionPlanMasks && Engine.DecisionPlan.ExclusivityGroupRoots.Length == Engine.DecisionPlan.Entries.Count;
            // Track which group roots have applied (per event) to skip remaining entries in same group when masking enabled.
            var appliedGroupRoots = masksEnabled ? new HashSet<int>() : null;
            if (Internal.FeatureFlags.EnableDecisionPlanGrouping && Engine.DecisionPlan.Groups.Count > 0)
            {
                // Grouped evaluation path: evaluate gate once per contiguous identical-condition group.
                foreach (var group in Engine.DecisionPlan.Groups)
                {
                    var gateEntry = Engine.DecisionPlan.Entries[group.StartIndex];
                    if (eventKindFiltering)
                    {
                        var groupKind = Engine.DecisionPlan.SupportedKinds.Length > group.StartIndex ? Engine.DecisionPlan.SupportedKinds[group.StartIndex] : EventKind.Any;
                        if (groupKind != EventKind.Any && (groupKind & currentEventKind) == 0)
                        {
                            continue; // skip group
                        }
                    }
                    bool gateValid = gateEntry.ConditionIsAlwaysValid || gateEntry.Condition.Evaluate(progress.State).Equals(ConditionResponse.Valid);
                    if (!gateValid)
                    {
                        continue; // skip entire group
                    }

                    for (var offset = 0; offset < group.Length; offset++)
                    {
                        var index = group.StartIndex + offset;
                        var entry = Engine.DecisionPlan.Entries[index];
                        if (masksEnabled)
                        {
                            var root = Engine.DecisionPlan.ExclusivityGroupRoots.Length > index ? Engine.DecisionPlan.ExclusivityGroupRoots[index] : -1;
                            if (root >= 0 && appliedGroupRoots.Contains(root))
                            {
                                continue; // another entry in this exclusivity group already applied
                            }
                        }
                        if (eventKindFiltering)
                        {
                            var ek = Engine.DecisionPlan.SupportedKinds.Length > index ? Engine.DecisionPlan.SupportedKinds[index] : EventKind.Any;
                            if (ek != EventKind.Any && (ek & currentEventKind) == 0)
                            {
                                continue;
                            }
                        }
                        // For first entry we've already evaluated the condition (unless always-valid); others share same reference so skip Evaluate.
                        if (offset > 0 && !entry.ConditionIsAlwaysValid)
                        {
                            // Skip re-evaluation (identical reference); condition already proven valid by gate.
                        }
                        else if (offset == 0)
                        {
                            // Already evaluated gate unless always-valid; nothing to do.
                        }

                        var ruleCheck = entry.Rule.Check(progress.Engine, progress.State, evt);
                        var observedPhase = entry.Phase ?? progress.Engine.GamePhaseRoot;
                        progress.Engine.Observer.OnPhaseEnter(observedPhase, progress.State);
                        progress.Engine.Observer.OnRuleEvaluated(observedPhase, entry.Rule, ruleCheck, progress.State, index);
                        if (ruleCheck.Result == ConditionResult.Valid)
                        {
                            var newState = entry.Rule.HandleEvent(progress.Engine, progress.State, evt);
                            progress.Engine.Observer.OnRuleApplied(observedPhase, entry.Rule, evt, progress.State, newState, index);
                            if (Internal.FeatureFlags.EnableStateHashing && newState.Hash.HasValue)
                            {
                                progress.Engine.Observer.OnStateHashed(newState, newState.Hash.Value);
                            }
                            progress = new GameProgress(progress.Engine, newState, progress.Events.Concat([evt]));
                            if (masksEnabled)
                            {
                                var root = Engine.DecisionPlan.ExclusivityGroupRoots.Length > index ? Engine.DecisionPlan.ExclusivityGroupRoots[index] : -1;
                                if (root >= 0)
                                {
                                    appliedGroupRoots.Add(root);
                                }
                            }
                            handled = true;
                            break;
                        }
                        else if (ruleCheck.Result == ConditionResult.Invalid)
                        {
                            throw new InvalidGameEventException(evt, ruleCheck, progress.Game, progress.Phase, progress.State);
                        }
                    }
                    if (handled)
                    {
                        break; // stop scanning remaining groups
                    }
                }
            }
            else
            {
                for (var i = 0; i < Engine.DecisionPlan.Entries.Count; i++)
                {
                    var entry = Engine.DecisionPlan.Entries[i];
                    if (masksEnabled)
                    {
                        var root = Engine.DecisionPlan.ExclusivityGroupRoots.Length > i ? Engine.DecisionPlan.ExclusivityGroupRoots[i] : -1;
                        if (root >= 0 && appliedGroupRoots.Contains(root))
                        {
                            continue;
                        }
                    }
                    if (eventKindFiltering)
                    {
                        var ek = Engine.DecisionPlan.SupportedKinds.Length > i ? Engine.DecisionPlan.SupportedKinds[i] : EventKind.Any;
                        if (ek != EventKind.Any && (ek & currentEventKind) == 0)
                        {
                            continue;
                        }
                    }
                    if (!entry.ConditionIsAlwaysValid && !entry.Condition.Evaluate(progress.State).Equals(ConditionResponse.Valid))
                    {
                        continue;
                    }
                    var ruleCheck = entry.Rule.Check(progress.Engine, progress.State, evt);
                    var observedPhase = entry.Phase ?? progress.Engine.GamePhaseRoot;
                    progress.Engine.Observer.OnPhaseEnter(observedPhase, progress.State);
                    progress.Engine.Observer.OnRuleEvaluated(observedPhase, entry.Rule, ruleCheck, progress.State, i);
                    if (ruleCheck.Result == ConditionResult.Valid)
                    {
                        var newState = entry.Rule.HandleEvent(progress.Engine, progress.State, evt);
                        progress.Engine.Observer.OnRuleApplied(observedPhase, entry.Rule, evt, progress.State, newState, i);
                        if (Internal.FeatureFlags.EnableStateHashing && newState.Hash.HasValue)
                        {
                            progress.Engine.Observer.OnStateHashed(newState, newState.Hash.Value);
                        }
                        progress = new GameProgress(progress.Engine, newState, progress.Events.Concat([evt]));
                        if (masksEnabled)
                        {
                            var root = Engine.DecisionPlan.ExclusivityGroupRoots.Length > i ? Engine.DecisionPlan.ExclusivityGroupRoots[i] : -1;
                            if (root >= 0)
                            {
                                appliedGroupRoots.Add(root);
                            }
                        }
                        handled = true;
                        break;
                    }
                    else if (ruleCheck.Result == ConditionResult.Invalid)
                    {
                        throw new InvalidGameEventException(evt, ruleCheck, progress.Game, progress.Phase, progress.State);
                    }
                }
            }

            if (!handled)
            {
                progress.Engine.Observer.OnEventIgnored(evt, progress.State);
            }
        }
        // Debug parity (dual-run) – shadow legacy evaluation and compare resulting state snapshots.
        if (debugParityEnabled)
        {
            var legacy = originalProgress.HandleEventLegacy(@event);
            var forceMismatch = Internal.Debug.DebugParityTestHooks.ForceMismatch; // test hook
            var equal = progress.State.Equals(legacy.State);
            if (!equal || forceMismatch)
            {
                // Compute simple diff summary (counts) for diagnostic clarity.
                var legacyStates = legacy.State.ChildStates.ToDictionary(s => s.Artifact.Id, s => s);
                var planStates = progress.State.ChildStates.ToDictionary(s => s.Artifact.Id, s => s);
                var mismatching = planStates.Keys.Union(legacyStates.Keys)
                    .Where(k => !legacyStates.ContainsKey(k) || !planStates.ContainsKey(k) || !legacyStates[k].Equals(planStates[k]))
                    .ToList();
                throw new BoardException($"DecisionPlan debug parity divergence detected (mismatched artifacts: {string.Join(", ", mismatching)}). ForceMismatch={(forceMismatch ? "true" : "false")}");
            }
        }
        return progress;
    }

    /// <summary>
    /// Legacy per-event tree traversal evaluation path (no DecisionPlan). Extracted for potential dual-run parity.
    /// </summary>
    private GameProgress HandleEventLegacy(IGameEvent @event)
    {
        var currentPhase = Engine.GamePhaseRoot.GetActiveGamePhase(State);
        if (currentPhase is null)
        {
            Engine.Observer.OnEventIgnored(@event, State);
            return this;
        }
        var events = currentPhase.PreProcessEvent(this, @event);
        return events.Aggregate(this, (seed, e) =>
        {
            var phaseForEvent = seed.Engine.GamePhaseRoot.GetActiveGamePhase(seed.State);
            if (phaseForEvent is null)
            {
                return seed; // nothing active; event ignored
            }
            seed.Engine.Observer.OnPhaseEnter(phaseForEvent, seed.State);
            var ruleCheckLocal = phaseForEvent.Rule.Check(seed.Engine, seed.State, e);
            seed.Engine.Observer.OnRuleEvaluated(phaseForEvent, phaseForEvent.Rule, ruleCheckLocal, seed.State, 0);
            if (ruleCheckLocal.Result == ConditionResult.Valid)
            {
                var newStateLocal = phaseForEvent.Rule.HandleEvent(seed.Engine, seed.State, e);
                seed.Engine.Observer.OnRuleApplied(phaseForEvent, phaseForEvent.Rule, e, seed.State, newStateLocal, 0);
                if (Internal.FeatureFlags.EnableStateHashing && newStateLocal.Hash.HasValue)
                {
                    seed.Engine.Observer.OnStateHashed(newStateLocal, newStateLocal.Hash.Value);
                }
                return new GameProgress(seed.Engine, newStateLocal, seed.Events.Concat([e]));
            }
            else if (ruleCheckLocal.Result == ConditionResult.Invalid)
            {
                throw new InvalidGameEventException(e, ruleCheckLocal, seed.Game, phaseForEvent, seed.State);
            }
            seed.Engine.Observer.OnEventIgnored(e, seed.State);
            return seed;
        });
    }
}