using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Applies a pass: increments numeric turn, increments pass streak, resets segment to Start, rotates active player (if present).
/// Inert when sequencing disabled or no current <see cref="TurnState"/> exists.
/// </summary>
internal sealed class TurnPassStateMutator : IStateMutator<TurnPassEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, TurnPassEvent @event)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return gameState;
        }

    TurnState? currentTurnState = null;
        foreach (var ts in gameState.GetStates<TurnState>()) { currentTurnState = ts; break; }
        if (currentTurnState is null) { return gameState; }

        var advancedTurnState = new TurnState(currentTurnState.Artifact, currentTurnState.TurnNumber + 1, TurnSegment.Start, currentTurnState.PassStreak + 1);
        return TurnSequencingHelpers.ApplyTurnAndRotate(engine, gameState, advancedTurnState);
    }
}