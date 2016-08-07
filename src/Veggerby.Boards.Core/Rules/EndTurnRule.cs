using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Phases;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    /* any event can trigger an EndTurnRule / EndTurnRule should therefore always be at the end
     * of the rule chain
     */
    public abstract class EndTurnRule : Rule<IGameEvent>
    {
        public override GameState Evaluate(Game game, GameState currentState, IGameEvent @event)
        {
            var turnState = currentState.GetActiveTurn();

            if (turnState == null)
            {
                throw new BoardException("No active turn to end");
            }

            var nextPlayer = game
                .Players
                .SkipWhile(x => !turnState.Artifact.Equals(x)) // find current player
                .Skip(1) // skip to next
                .FirstOrDefault();
        
            Turn nextTurn = null;

            if (nextPlayer == null)
            {
                var round = new Round(turnState.Round.Number + 1);
                nextTurn = new Turn(round, 1);
                nextPlayer = game.Players.First();
            }
            else
            {
                nextTurn = new Turn(turnState.Round, turnState.Turn.Number + 1);
            }

            return currentState.Update(new TurnState(nextPlayer, nextTurn));
        }
    }
}