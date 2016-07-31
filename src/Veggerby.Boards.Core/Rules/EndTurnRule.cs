using System.Linq;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Phases;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class EndTurnRule : Rule<EndTurnGameEvent>
    {
        public override GameState Evaluate(GameEngine gameEngine, GameState currentState, EndTurnGameEvent @event)
        {
            var turnState = currentState.GetActiveTurn();

            if (turnState == null)
            {
                throw new BoardException("No active turn to end");
            }

            var nextPlayer = gameEngine
                .Game
                .Players
                .SkipWhile(x => !turnState.Artifact.Equals(x)) // find current player
                .Skip(1) // skip to next
                .FirstOrDefault();
        
            Turn nextTurn = null;

            if (nextPlayer == null)
            {
                var round = new Round(turnState.Round.Number + 1);
                nextTurn = new Turn(round, 1);
                nextPlayer = gameEngine.Game.Players.First();
            }
            else
            {
                nextTurn = new Turn(turnState.Round, turnState.Turn.Number + 1);
            }

            return currentState.Set(new TurnState(nextPlayer, nextTurn));
        }
    }
}