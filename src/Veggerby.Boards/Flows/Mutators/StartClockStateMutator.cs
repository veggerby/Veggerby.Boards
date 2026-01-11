using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator that starts a player's clock when their turn begins.
/// </summary>
public sealed class StartClockStateMutator : IStateMutator<StartClockEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, StartClockEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var clockState = gameState.GetStates<ClockState>()
            .FirstOrDefault(cs => cs.Clock.Equals(@event.Clock));

        if (clockState == null)
        {
            return gameState;
        }

        var newClockState = clockState.StartTurn(@event.Player, @event.Timestamp);

        return gameState.Next(new[] { newClockState });
    }
}
