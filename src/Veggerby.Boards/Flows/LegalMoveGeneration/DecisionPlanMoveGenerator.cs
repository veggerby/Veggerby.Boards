using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.LegalMoveGeneration;

/// <summary>
/// Default implementation of <see cref="ILegalMoveGenerator"/> using the compiled <see cref="DecisionPlan.DecisionPlan"/>.
/// </summary>
/// <remarks>
/// <para>
/// This generator leverages the engine's precompiled decision plan to efficiently validate moves
/// without repeated rule tree traversal. It integrates with module-specific candidate generators
/// to enumerate possible events, then filters through the decision plan's rule evaluation.
/// </para>
/// <para>
/// Implementation strategy:
/// <list type="number">
/// <item><description>Determine active phases from current state</description></item>
/// <item><description>For each phase, generate candidate events (module-specific)</description></item>
/// <item><description>Validate each candidate through decision plan evaluation</description></item>
/// <item><description>Yield only events that pass validation</description></item>
/// </list>
/// </para>
/// <para>
/// This class is public to allow module-specific subclassing for candidate generation.
/// External code should typically use the <see cref="GameProgressExtensions.GetLegalMoveGenerator"/>
/// extension method to obtain an instance.
/// </para>
/// </remarks>
public class DecisionPlanMoveGenerator : ILegalMoveGenerator
{
    /// <summary>
    /// Gets the game engine containing the compiled decision plan.
    /// </summary>
    protected GameEngine Engine { get; }

    /// <summary>
    /// Gets the current game state to generate moves for.
    /// </summary>
    protected GameState State { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DecisionPlanMoveGenerator"/> class.
    /// </summary>
    /// <param name="engine">The game engine containing the compiled decision plan.</param>
    /// <param name="state">The current game state to generate moves for.</param>
    public DecisionPlanMoveGenerator(GameEngine engine, GameState state)
    {
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <inheritdoc />
    public virtual IEnumerable<IGameEvent> GetLegalMoves(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Check if game has ended
        if (state.GetStates<GameEndedState>().Any())
        {
            yield break;
        }

        // Get active phase
        var activePhase = Engine.GamePhaseRoot.GetActiveGamePhase(state);

        if (activePhase is null)
        {
            yield break;
        }

        // Generate candidate events and filter through validation
        // For now, we'll use a simple approach: try validation on generated candidates
        // Module-specific generators can override GenerateCandidates to provide actual moves

        foreach (var candidate in GenerateCandidates(state, activePhase))
        {
            var validation = Validate(candidate, state);

            if (validation.IsLegal)
            {
                yield return candidate;
            }
        }
    }

    /// <inheritdoc />
    public virtual MoveValidation Validate(IGameEvent @event, GameState state)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(state);

        // Check if game has ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return MoveValidation.Illegal(@event, RejectionReason.GameEnded, "Game has already ended");
        }

        // Get active phase
        var activePhase = Engine.GamePhaseRoot.GetActiveGamePhase(state);

        if (activePhase is null)
        {
            return MoveValidation.Illegal(@event, RejectionReason.WrongPhase, "No active phase available for event");
        }

        // Pre-process the event
        var preProcessed = activePhase.PreProcessEvent(new GameProgress(Engine, state, null), @event);

        // Try each preprocessed event against the decision plan
        foreach (var evt in preProcessed)
        {
            // Iterate through decision plan entries
            for (var i = 0; i < Engine.DecisionPlan.Entries.Count; i++)
            {
                var entry = Engine.DecisionPlan.Entries[i];

                // Check phase condition
                if (!entry.ConditionIsAlwaysValid && !entry.Condition.Evaluate(state).Equals(ConditionResponse.Valid))
                {
                    continue;
                }

                // Check rule
                var ruleCheck = entry.Rule.Check(Engine, state, evt);

                if (ruleCheck.Result == ConditionResult.Valid)
                {
                    // Move is legal
                    return MoveValidation.Legal(@event);
                }
                else if (ruleCheck.Result == ConditionResult.Invalid)
                {
                    // Move explicitly rejected - map to rejection reason
                    var reason = MapConditionToRejection(ruleCheck, evt);
                    var explanation = ruleCheck.Reason ?? "Invalid move";
                    return MoveValidation.Illegal(@event, reason, explanation);
                }
                // If Ignore, continue to next rule
            }
        }

        // No rule accepted the event
        return MoveValidation.Illegal(@event, RejectionReason.WrongPhase, "No rule accepted the event");
    }

    /// <inheritdoc />
    public virtual IEnumerable<IGameEvent> GetLegalMovesFor(Artifact artifact, GameState state)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentNullException.ThrowIfNull(state);

        // Filter legal moves to those involving the specified artifact
        foreach (var move in GetLegalMoves(state))
        {
            if (EventInvolvesArtifact(move, artifact))
            {
                yield return move;
            }
        }
    }

    /// <summary>
    /// Generates candidate events for the current state and phase.
    /// </summary>
    /// <remarks>
    /// This is an extensibility point for module-specific move generation.
    /// Base implementation returns empty enumeration; subclasses should override to provide actual candidates.
    /// </remarks>
    /// <param name="state">Current game state.</param>
    /// <param name="phase">Active phase.</param>
    /// <returns>Enumerable of candidate events.</returns>
    protected virtual IEnumerable<IGameEvent> GenerateCandidates(GameState state, Phases.GamePhase phase)
    {
        // Placeholder for candidate generation
        // Modules can override this method to provide actual candidates
        // For now, return empty to allow base validation to work
        return Enumerable.Empty<IGameEvent>();
    }

    /// <summary>
    /// Checks if an event involves a specific artifact.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <param name="artifact">The artifact to match.</param>
    /// <returns>True if the event involves the artifact; false otherwise.</returns>
    protected virtual bool EventInvolvesArtifact(IGameEvent @event, Artifact artifact)
    {
        // Check common event types
        if (@event is MovePieceGameEvent moveEvent)
        {
            return moveEvent.Piece == artifact;
        }

        // Add more event type checks as needed
        // For now, return false for unknown event types
        return false;
    }

    /// <summary>
    /// Maps a condition response to a structured rejection reason.
    /// </summary>
    /// <param name="response">The condition response.</param>
    /// <param name="event">The event being validated.</param>
    /// <returns>The corresponding rejection reason.</returns>
    protected static RejectionReason MapConditionToRejection(ConditionResponse response, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(response);

        var reason = response.Reason ?? string.Empty;

        // Map based on reason string patterns
        // This heuristic approach works with existing condition messages

        if (reason.Contains("not owned", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("not your", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("active player", StringComparison.OrdinalIgnoreCase))
        {
            return RejectionReason.PieceNotOwned;
        }

        if (reason.Contains("obstructed", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("blocked", StringComparison.OrdinalIgnoreCase))
        {
            return RejectionReason.PathObstructed;
        }

        if (reason.Contains("occupied", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("not empty", StringComparison.OrdinalIgnoreCase))
        {
            return RejectionReason.DestinationOccupied;
        }

        if (reason.Contains("no valid path", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("invalid pattern", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("movement pattern", StringComparison.OrdinalIgnoreCase))
        {
            return RejectionReason.InvalidPattern;
        }

        if (reason.Contains("phase", StringComparison.OrdinalIgnoreCase))
        {
            return RejectionReason.WrongPhase;
        }

        if (reason.Contains("dice", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("insufficient", StringComparison.OrdinalIgnoreCase))
        {
            return RejectionReason.InsufficientResources;
        }

        // Default to generic rule violation
        return RejectionReason.RuleViolation;
    }
}
