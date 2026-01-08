using System;
using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator handling <see cref="RevealCommitmentsEvent"/> by resolving all staged commitments
/// simultaneously in deterministic order.
/// </summary>
/// <remarks>
/// Applies all committed events in player ID order (ascending) to ensure deterministic outcomes.
/// After application, clears the staged events state. If no <see cref="StagedEventsState"/> exists
/// or commitments are incomplete, this mutator is inert (no-op).
/// </remarks>
internal sealed class RevealCommitmentsStateMutator : IStateMutator<RevealCommitmentsEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, RevealCommitmentsEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        StagedEventsState? currentStaged = null;
        foreach (var state in gameState.GetStates<StagedEventsState>())
        {
            currentStaged = state;
            break;
        }

        if (currentStaged is null)
        {
            return gameState;
        }

        if (!currentStaged.IsComplete)
        {
            return gameState;
        }

        // Apply committed events in deterministic order (player ID ascending)
        var orderedCommitments = currentStaged.Commitments
            .OrderBy(kvp => kvp.Key.Id, StringComparer.Ordinal)
            .ToList();

        var resultState = gameState;
        foreach (var (player, committedEvent) in orderedCommitments)
        {
            // Apply each committed event using the engine's rule evaluation
            // Note: We use a GameProgress wrapper to apply events through the normal rule pipeline
            var tempProgress = new GameProgress(engine, resultState, null);
            var handledProgress = tempProgress.HandleEvent(committedEvent);
            resultState = handledProgress.State;
        }

        // Clear the staged events by removing the state (commitment phase ends)
        // We need to remove the StagedEventsState artifact from the result state
        var statesWithoutStaged = resultState.ChildStates
            .Where(s => s is not StagedEventsState)
            .ToList();

        return GameState.New(statesWithoutStaged, resultState.Random);
    }
}
