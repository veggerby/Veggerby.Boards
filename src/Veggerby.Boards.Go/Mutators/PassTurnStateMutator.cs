using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go.Mutators;

/// <summary>
/// Applies a <see cref="PassTurnGameEvent"/> by incrementing consecutive pass count in extras.
/// </summary>
public sealed class PassTurnStateMutator : IStateMutator<PassTurnGameEvent>
{
    /// <summary>
    /// Increments the consecutive pass counter (used for end-of-game detection) in Go extras state.
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, PassTurnGameEvent @event)
    {
        var extras = gameState.GetExtras<GoStateExtras>() ?? new GoStateExtras(null, 0, 19);
        var updated = extras with { ConsecutivePasses = extras.ConsecutivePasses + 1 };
        return gameState.ReplaceExtras(updated);
    }
}