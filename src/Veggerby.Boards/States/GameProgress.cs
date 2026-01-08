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
public partial class GameProgress
{
    /// <summary>
    /// Initializes a new <see cref="GameProgress"/> instance.
    /// </summary>
    /// <param name="engine">The game engine hosting rules and phase graph.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="events">The historical events (optional).</param>
    internal GameProgress(GameEngine engine, GameState state, Veggerby.Boards.Flows.Events.EventChain? events)
    {
        ArgumentNullException.ThrowIfNull(engine);

        ArgumentNullException.ThrowIfNull(state);

        Engine = engine;
        State = state;
        Events = events ?? Veggerby.Boards.Flows.Events.EventChain.Empty;
        Phase = Engine.GamePhaseRoot.GetActiveGamePhase(State);
        // Acceleration snapshots now encapsulated inside EngineCapabilities.AccelerationContext
    }

    /// <summary>
    /// Gets the engine context.
    /// </summary>
    public GameEngine Engine
    {
        get;
    }

    /// <summary>
    /// Gets the current state snapshot.
    /// </summary>
    public GameState State
    {
        get;
    }

    /// <summary>
    /// Gets the active phase derived from the phase tree and current state.
    /// </summary>
    public GamePhase? Phase
    {
        get;
    }

    /// <summary>
    /// Gets the event history up to this progress state.
    /// </summary>
    public Veggerby.Boards.Flows.Events.EventChain Events
    {
        get;
    }

    // Removed internal snapshot exposure (non-leaky acceleration context now authoritative)

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
        // For manual state creation path we fall back to full rebuild if piece map acceleration is enabled.
        var newState = State.Next(newStates);
        Engine.Capabilities?.AccelerationContext?.OnStateTransition(State, newState, new Flows.Events.NullGameEvent());
        return new GameProgress(Engine, newState, Events);
    }

    /// <summary>
    /// Handles a game event (with pre-processing) producing a new immutable progress instance.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>The resulting progress.</returns>
    public GameProgress HandleEvent(IGameEvent @event)
    {
        // DecisionPlan always enabled: iterate precompiled leaf phases in order, selecting first whose condition is valid.
        var originalProgress = this; // retained for potential future shadow instrumentation (now legacy removed)
        var debugParityEnabled = false; // legacy parity mode removed
        // to pre-process per candidate phase if needed). This maintains functional parity with legacy traversal.
        // Phase can be null if no active phase resolves for current state; in that case, skip pre-processing.
        var preProcessed = Phase is null ? [@event] : Phase.PreProcessEvent(this, @event);
        var progress = this;
        foreach (var evt in preProcessed)
        {
            var handled = false;
            // Determine conflict resolution strategy (use first entry's strategy, or default to FirstWins if no entries)
            var strategy = Engine.DecisionPlan.Entries.Count > 0
                ? Engine.DecisionPlan.Entries[0].ConflictResolution
                : ConflictResolutionStrategy.FirstWins;

            // Optimize for FirstWins (default, backward compatible) - use simple linear scan
            if (strategy == ConflictResolutionStrategy.FirstWins)
            {
                for (var i = 0; i < Engine.DecisionPlan.Entries.Count; i++)
                {
                    var entry = Engine.DecisionPlan.Entries[i];

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
                        progress.Engine.Capabilities?.AccelerationContext?.OnStateTransition(progress.State, newState, evt);

                        if (newState.Hash.HasValue)
                        {
                            progress.Engine.Observer.OnStateHashed(newState, newState.Hash.Value);
                        }

                        progress.Engine.Observer.OnRuleApplied(observedPhase, entry.Rule, evt, progress.State, newState, i);

                        // Apply endgame detection if configured for this phase
                        if (entry.Phase != null)
                        {
                            newState = entry.Phase.CheckAndApplyEndGame(progress.Engine, newState, evt);
                        }

                        progress = new GameProgress(progress.Engine, newState, progress.Events.Append(evt));

                        handled = true;
                        break;
                    }
                    else if (ruleCheck.Result == ConditionResult.Invalid)
                    {
                        throw new InvalidGameEventException(evt, ruleCheck, progress.Game, progress.Phase ?? progress.Engine.GamePhaseRoot, progress.State);
                    }
                }
            }
            else
            {
                // Collect all matching rules for conflict resolution
                var candidates = new List<(int Index, Flows.DecisionPlan.DecisionPlanEntry Entry, ConditionResponse Check)>();
                for (var i = 0; i < Engine.DecisionPlan.Entries.Count; i++)
                {
                    var entry = Engine.DecisionPlan.Entries[i];

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
                        candidates.Add((i, entry, ruleCheck));
                    }
                    else if (ruleCheck.Result == ConditionResult.Invalid)
                    {
                        throw new InvalidGameEventException(evt, ruleCheck, progress.Game, progress.Phase ?? progress.Engine.GamePhaseRoot, progress.State);
                    }
                }

                // Apply conflict resolution strategy
                if (candidates.Count > 0)
                {
                    handled = true;
                    progress = ApplyConflictResolution(progress, evt, candidates, strategy);
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
#if DEBUG || TESTS
#pragma warning disable CS0618 // intentional legacy parity call
            var legacy = originalProgress.HandleEventLegacy(@event);
#pragma warning restore CS0618
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
#else
            // Parity requested but legacy handler not included in this build configuration.
#endif
        }

        return progress;
    }

    /// <summary>
    /// Handles an event and returns a structured <see cref="EventResult"/> indicating success or rejection reason.
    /// </summary>
    /// <param name="event">Event to evaluate.</param>
    /// <returns>Typed handling result (never null).</returns>
    public EventResult HandleEventResult(IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // If there is no active phase resolved for current state, treat as phase closed (no rule set is eligible).
        if (Phase is null)
        {
            return EventResult.Rejected(State, EventRejectionReason.PhaseClosed, "No active phase available for event.");
        }

        var before = State;
        try
        {
            var after = HandleEvent(@event); // may throw
            if (!ReferenceEquals(before, after.State) && !before.Equals(after.State))
            {
                return EventResult.Accepted(after.State);
            }

            return EventResult.Rejected(before, EventRejectionReason.NotApplicable, "No rule produced a state change.");
        }
        catch (InvalidGameEventException ex)
        {
            return EventResult.Rejected(before, EventRejectionReason.RuleRejected, ex.ConditionResponse?.Reason);
        }
        catch (BoardException ex)
        {
            var msg = ex.Message ?? string.Empty;
            var reason = EventRejectionReason.InvalidEvent;
            if (msg.Contains("No valid dice state for path", StringComparison.OrdinalIgnoreCase))
            {
                reason = EventRejectionReason.PathNotFound;
            }
            else if (msg.Contains("Invalid from tile", StringComparison.OrdinalIgnoreCase))
            {
                reason = EventRejectionReason.InvalidOwnership;
            }

            return EventResult.Rejected(before, reason, ex.Message);
        }
        catch (Exception ex)
        {
            return EventResult.Rejected(before, EventRejectionReason.EngineInvariant, ex.Message);
        }
    }

    /// <summary>
    /// Applies conflict resolution strategy to select and execute the appropriate rule(s) from candidates.
    /// </summary>
    /// <param name="progress">Current progress.</param>
    /// <param name="evt">Event being processed.</param>
    /// <param name="candidates">List of matching rule entries with their indices and check results.</param>
    /// <param name="strategy">Conflict resolution strategy to apply.</param>
    /// <returns>Updated progress after rule execution.</returns>
    private GameProgress ApplyConflictResolution(
        GameProgress progress,
        IGameEvent evt,
        List<(int Index, Flows.DecisionPlan.DecisionPlanEntry Entry, ConditionResponse Check)> candidates,
        ConflictResolutionStrategy strategy)
    {
        if (candidates.Count == 0)
        {
            return progress;
        }

        switch (strategy)
        {
            case ConflictResolutionStrategy.HighestPriority:
            {
                // Sort by priority (descending), then by declaration order (ascending index for ties)
                candidates.Sort((a, b) =>
                {
                    var priorityCompare = b.Entry.Priority.CompareTo(a.Entry.Priority);
                    return priorityCompare != 0 ? priorityCompare : a.Index.CompareTo(b.Index);
                });

                // Select the highest priority rule (first after sort)
                var selected = candidates[0];
                return ApplyRule(progress, evt, selected.Entry, selected.Index);
            }

            case ConflictResolutionStrategy.LastWins:
            {
                // Select the last matching rule (highest index)
                var selected = candidates[candidates.Count - 1];
                return ApplyRule(progress, evt, selected.Entry, selected.Index);
            }

            case ConflictResolutionStrategy.Exclusive:
            {
                // Fail-fast if multiple rules match
                if (candidates.Count > 1)
                {
                    var ruleNames = string.Join(", ", candidates.Select(c =>
                        c.Entry.Phase?.Label ?? c.Entry.Rule.GetType().Name));
                    throw new BoardException(
                        $"Exclusive conflict resolution failed: {candidates.Count} rules matched for event {evt.GetType().Name}. Conflicting rules: {ruleNames}");
                }

                var selected = candidates[0];
                return ApplyRule(progress, evt, selected.Entry, selected.Index);
            }

            case ConflictResolutionStrategy.ApplyAll:
            {
                // Apply all rules in priority order (highest first), threading state through each
                candidates.Sort((a, b) =>
                {
                    var priorityCompare = b.Entry.Priority.CompareTo(a.Entry.Priority);
                    return priorityCompare != 0 ? priorityCompare : a.Index.CompareTo(b.Index);
                });

                foreach (var candidate in candidates)
                {
                    progress = ApplyRule(progress, evt, candidate.Entry, candidate.Index);
                }

                return progress;
            }

            case ConflictResolutionStrategy.FirstWins:
            default:
            {
                // Should not reach here (FirstWins is optimized in HandleEvent), but handle defensively
                var selected = candidates[0];
                return ApplyRule(progress, evt, selected.Entry, selected.Index);
            }
        }
    }

    /// <summary>
    /// Applies a single rule entry to the current progress, producing a new state.
    /// </summary>
    /// <param name="progress">Current progress.</param>
    /// <param name="evt">Event being processed.</param>
    /// <param name="entry">Decision plan entry to apply.</param>
    /// <param name="entryIndex">Index of entry in decision plan (for observer notifications).</param>
    /// <returns>New progress with updated state.</returns>
    private GameProgress ApplyRule(
        GameProgress progress,
        IGameEvent evt,
        Flows.DecisionPlan.DecisionPlanEntry entry,
        int entryIndex)
    {
        var newState = entry.Rule.HandleEvent(progress.Engine, progress.State, evt);
        progress.Engine.Capabilities?.AccelerationContext?.OnStateTransition(progress.State, newState, evt);

        if (newState.Hash.HasValue)
        {
            progress.Engine.Observer.OnStateHashed(newState, newState.Hash.Value);
        }

        var observedPhase = entry.Phase ?? progress.Engine.GamePhaseRoot;
        progress.Engine.Observer.OnRuleApplied(observedPhase, entry.Rule, evt, progress.State, newState, entryIndex);

        // Apply endgame detection if configured for this phase
        if (entry.Phase != null)
        {
            newState = entry.Phase.CheckAndApplyEndGame(progress.Engine, newState, evt);
        }

        return new GameProgress(progress.Engine, newState, progress.Events.Append(evt));
    }
}