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

            var nextPlayer = gameEngine
                .Game
                .Players
                .SkipWhile(x => !(turnState?.Artifact.Equals(x) ?? false)) // find current player
                .Skip(1) // skip to next
                .FirstOrDefault();
        
            Turn nextTurn = null;

            if (nextPlayer == null)
            {
                var round = new Round((turnState.Round?.Number ?? 0) + 1);
                nextTurn = new Turn(round, 1);
                nextPlayer = gameEngine.Game.Players.First();
            }
            else
            {
                nextTurn = new Turn(turnState.Round, turnState.Turn.Number + 1);
            }

            return currentState.Set(
                nextPlayer, 
                state => gameEngine.EvaluateTurnState(nextPlayer, nextTurn));
        }
    }
}