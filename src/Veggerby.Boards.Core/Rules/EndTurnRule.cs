using System.Linq;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Phases;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class EndTurnRule : Rule<EndTurnGameEvent>
    {
        public abstract bool IsEndOfTurn(GameState currentState);

        public override GameState GetState(GameEngine gameEngine, GameState currentState, EndTurnGameEvent @event)
        {
            if (IsEndOfTurn(currentState))
            {
                var turn = currentState.ActiveTurn;
                var nextPlayer = gameEngine
                    .Players
                    .SkipWhile(x => !(turn?.Turn.Player.Equals(x) ?? false)) // find current player
                    .Skip(1) // skip to next
                    .FirstOrDefault();
            
                Turn nextTurn = null;

                if (nextPlayer == null)
                {
                    var round = new Round((turn.Round?.Number ?? 0) + 1);
                    nextTurn = new Turn(round, gameEngine.Players.First());

                }
                else
                {
                    nextTurn = new Turn(turn.Round, nextPlayer);
                }

                var turnState = gameEngine.EvaluateTurnState(nextTurn);

                return currentState.NextTurn(turnState);
            }

            return null;
        }
    }
}