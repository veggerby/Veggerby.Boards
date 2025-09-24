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
    /// <param name="pieceMapSnapshot">Optional piece map acceleration snapshot (internal) carried forward for incremental updates.</param>
    /// <param name="bitboardSnapshot">Optional bitboard occupancy snapshot (global + per-player) carried forward for incremental updates.</param>
    internal GameProgress(GameEngine engine, GameState state, IEnumerable<IGameEvent> events, Internal.Layout.PieceMapSnapshot pieceMapSnapshot = null, Internal.Layout.BitboardSnapshot bitboardSnapshot = null)
    {
        ArgumentNullException.ThrowIfNull(engine);

        ArgumentNullException.ThrowIfNull(state);

        Engine = engine;
        State = state;
        Events = [.. (events ?? Enumerable.Empty<IGameEvent>())];
        Phase = Engine.GamePhaseRoot.GetActiveGamePhase(State);
        _pieceMapSnapshot = pieceMapSnapshot; // may be null if feature disabled
        _bitboardSnapshot = bitboardSnapshot; // may be null if feature disabled
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

    private readonly Internal.Layout.PieceMapSnapshot _pieceMapSnapshot;

    /// <summary>
    /// Gets the acceleration PieceMap snapshot for this progress (internal experimental).
    /// </summary>
    internal Internal.Layout.PieceMapSnapshot PieceMapSnapshot => _pieceMapSnapshot;

    private readonly Internal.Layout.BitboardSnapshot _bitboardSnapshot;

    /// <summary>
    /// Gets the incremental bitboard snapshot (global + per-player occupancy) if enabled.
    /// </summary>
    internal Internal.Layout.BitboardSnapshot BitboardSnapshot => _bitboardSnapshot;

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
        Internal.Layout.PieceMapSnapshot nextSnapshot = _pieceMapSnapshot;
        Internal.Layout.BitboardSnapshot nextBb = _bitboardSnapshot;
        if (Engine.Services.TryGet(out Internal.Layout.PieceMapServices pServices))
        {
            // Full rebuild since we don't know which pieces changed.
            nextSnapshot = Internal.Layout.PieceMapSnapshot.Build(pServices.Layout, State.Next(newStates), Engine.Services.TryGet(out Internal.Layout.BoardShape shape) ? shape : null);
        }

        if (Engine.Services.TryGet(out Internal.Layout.BitboardServices bbServices) && Engine.Services.TryGet(out Internal.Layout.BoardShape shape2))
        {
            nextBb = Internal.Layout.BitboardSnapshot.Build(bbServices.Layout, State.Next(newStates), shape2);
        }
        return new GameProgress(Engine, State.Next(newStates), Events, nextSnapshot, nextBb);
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
                            var nextSnapshot = progress._pieceMapSnapshot;
                            var nextBb = progress._bitboardSnapshot;

                            if (progress.Engine.Services.TryGet(out Internal.Layout.PieceMapServices pServices) && progress.Engine.Services.TryGet(out Internal.Layout.BoardShape shape))
                            {
                                if (evt is MovePieceGameEvent mpe)
                                {
                                    if (shape.TryGetTileIndex(mpe.From, out var fromIdx) && shape.TryGetTileIndex(mpe.To, out var toIdx))
                                    {
                                        nextSnapshot = nextSnapshot?.UpdateForMove(mpe.Piece, (short)fromIdx, (short)toIdx)
                                            ?? Internal.Layout.PieceMapSnapshot.Build(pServices.Layout, newState, shape);
                                        if (progress.Engine.Services.TryGet(out Internal.Layout.BitboardServices bbServices2))
                                        {
                                            nextBb = (nextBb ?? Internal.Layout.BitboardSnapshot.Build(bbServices2.Layout, newState, shape))
                                                .UpdateForMove(mpe.Piece, (short)fromIdx, (short)toIdx, nextSnapshot, shape);
                                        }
                                    }
                                }
                                else
                                {
                                    nextSnapshot ??= Internal.Layout.PieceMapSnapshot.Build(pServices.Layout, newState, shape);
                                    if (progress.Engine.Services.TryGet(out Internal.Layout.BitboardServices bbServices3))
                                    {
                                        nextBb = Internal.Layout.BitboardSnapshot.Build(bbServices3.Layout, newState, shape);
                                    }
                                }
                            }

                            progress.Engine.Observer.OnRuleApplied(observedPhase, entry.Rule, evt, progress.State, newState, index);

                            if (Internal.FeatureFlags.EnableStateHashing && newState.Hash.HasValue)
                            {
                                progress.Engine.Observer.OnStateHashed(newState, newState.Hash.Value);
                            }

                            progress = new GameProgress(progress.Engine, newState, progress.Events.Concat([evt]), nextSnapshot, nextBb);

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
                        var nextSnapshot = progress._pieceMapSnapshot;
                        var nextBb = progress._bitboardSnapshot;

                        if (progress.Engine.Services.TryGet(out Internal.Layout.PieceMapServices pServices2) && progress.Engine.Services.TryGet(out Internal.Layout.BoardShape shape2))
                        {
                            if (evt is MovePieceGameEvent mpe2)
                            {
                                if (shape2.TryGetTileIndex(mpe2.From, out var fromIdx2) && shape2.TryGetTileIndex(mpe2.To, out var toIdx2))
                                {
                                    nextSnapshot = nextSnapshot?.UpdateForMove(mpe2.Piece, (short)fromIdx2, (short)toIdx2)
                                        ?? Internal.Layout.PieceMapSnapshot.Build(pServices2.Layout, newState, shape2);
                                    if (progress.Engine.Services.TryGet(out Internal.Layout.BitboardServices bbServices4))
                                    {
                                        nextBb = (nextBb ?? Internal.Layout.BitboardSnapshot.Build(bbServices4.Layout, newState, shape2))
                                            .UpdateForMove(mpe2.Piece, (short)fromIdx2, (short)toIdx2, nextSnapshot, shape2);
                                    }
                                }
                            }
                            else
                            {
                                nextSnapshot ??= Internal.Layout.PieceMapSnapshot.Build(pServices2.Layout, newState, shape2);
                                if (progress.Engine.Services.TryGet(out Internal.Layout.BitboardServices bbServices5))
                                {
                                    nextBb = Internal.Layout.BitboardSnapshot.Build(bbServices5.Layout, newState, shape2);
                                }
                            }
                        }
                        progress.Engine.Observer.OnRuleApplied(observedPhase, entry.Rule, evt, progress.State, newState, i);

                        if (Internal.FeatureFlags.EnableStateHashing && newState.Hash.HasValue)
                        {
                            progress.Engine.Observer.OnStateHashed(newState, newState.Hash.Value);
                        }

                        progress = new GameProgress(progress.Engine, newState, progress.Events.Concat([evt]), nextSnapshot, nextBb);

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
    /// Handles an event and returns a structured <see cref="Flows.Events.EventResult"/> indicating success or rejection reason.
    /// </summary>
    /// <param name="event">Event to evaluate.</param>
    /// <returns>Typed handling result (never null).</returns>
    public Flows.Events.EventResult HandleEventResult(IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // If there is no active phase, treat as phase closed (cannot apply any rule).
        if (Phase is null)
        {
            return Flows.Events.EventResult.Rejected(State, Flows.Events.EventRejectionReason.PhaseClosed, "No active phase accepts the event.");
        }

        return Internal.EventRejectionClassifier.Classify(this, @event);
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

                var nextSnapshot = seed._pieceMapSnapshot;
                var nextBb = seed._bitboardSnapshot;
                if (seed.Engine.Services.TryGet(out Internal.Layout.PieceMapServices pServices3) && seed.Engine.Services.TryGet(out Internal.Layout.BoardShape shape3))
                {
                    if (e is MovePieceGameEvent mpe3)
                    {
                        if (shape3.TryGetTileIndex(mpe3.From, out var fromIdx3) && shape3.TryGetTileIndex(mpe3.To, out var toIdx3))
                        {
                            nextSnapshot = nextSnapshot?.UpdateForMove(mpe3.Piece, (short)fromIdx3, (short)toIdx3)
                                ?? Internal.Layout.PieceMapSnapshot.Build(pServices3.Layout, newStateLocal, shape3);
                            if (seed.Engine.Services.TryGet(out Internal.Layout.BitboardServices bbServices6))
                            {
                                nextBb = (nextBb ?? Internal.Layout.BitboardSnapshot.Build(bbServices6.Layout, newStateLocal, shape3))
                                    .UpdateForMove(mpe3.Piece, (short)fromIdx3, (short)toIdx3, nextSnapshot, shape3);
                            }
                        }
                    }
                    else
                    {
                        nextSnapshot ??= Internal.Layout.PieceMapSnapshot.Build(pServices3.Layout, newStateLocal, shape3);
                        if (seed.Engine.Services.TryGet(out Internal.Layout.BitboardServices bbServices7))
                        {
                            nextBb = Internal.Layout.BitboardSnapshot.Build(bbServices7.Layout, newStateLocal, shape3);
                        }
                    }
                }
                return new GameProgress(seed.Engine, newStateLocal, seed.Events.Concat([e]), nextSnapshot, nextBb);
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