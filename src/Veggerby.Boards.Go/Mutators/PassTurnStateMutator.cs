using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go.Mutators;

/// <summary>
/// Applies a <see cref="PassTurnGameEvent"/> by incrementing consecutive pass count in extras.
/// When two consecutive passes occur, adds a <see cref="GameEndedState"/> to mark game termination.
/// </summary>
public sealed class PassTurnStateMutator : IStateMutator<PassTurnGameEvent>
{
    /// <summary>
    /// Increments the consecutive pass counter (used for end-of-game detection) in Go extras state.
    /// On second consecutive pass, marks the game as ended.
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, PassTurnGameEvent @event)
    {
        var extras = gameState.GetExtras<GoStateExtras>() ?? new GoStateExtras(null, 0, 19);
        var newPassCount = extras.ConsecutivePasses + 1;
        var updated = extras with
        {
            ConsecutivePasses = newPassCount,
            KoTileId = null  // Clear ko on pass
        };

        var stateWithExtras = gameState.ReplaceExtras(updated);

        // If two consecutive passes, mark game as ended
        if (newPassCount >= 2)
        {
            return stateWithExtras.Next([new GameEndedState()]);
        }

        return stateWithExtras;
    }
}