using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class EndTurnRule : Rule<EndTurnGameEvent>
    {
        public abstract bool IsEndOfTurn(GameState currentState);

        public override GameState GetState(GameState currentState, EndTurnGameEvent @event)
        {
            if (IsEndOfTurn(currentState))
            {
            }

            return null;
        }
    }
}