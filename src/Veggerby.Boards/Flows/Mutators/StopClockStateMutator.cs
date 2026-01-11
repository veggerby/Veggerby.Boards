using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator that stops a player's clock and applies time deductions/increments.
/// </summary>
public sealed class StopClockStateMutator : IStateMutator<StopClockEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, StopClockEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var clockState = gameState.GetStates<ClockState>()
            .FirstOrDefault(cs => cs.Clock.Equals(@event.Clock));

        if (clockState == null)
        {
            return gameState;
        }

        var newClockState = clockState.EndTurn(@event.Timestamp);

        return gameState.Next(new[] { newClockState });
    }
}
