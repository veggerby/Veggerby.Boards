using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

public class DiceStateMutator<T> : IStateMutator<RollDiceGameEvent<T>>
{
    public GameState MutateState(GameEngine engine, GameState gameState, RollDiceGameEvent<T> @event)
    {
        return gameState.Next(@event.NewDiceStates);
    }
}