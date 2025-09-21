using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

/// <summary>
/// Applies dice state updates produced by a roll event.
/// </summary>
public class DiceStateMutator<T> : IStateMutator<RollDiceGameEvent<T>>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, RollDiceGameEvent<T> @event)
    {
        return gameState.Next(@event.NewDiceStates);
    }
}