using Veggerby.Boards.Core.Phases;

namespace Veggerby.Boards.Core.States
{
    public class TurnState
    {
        public GamePhase GamePhase { get; }
        public Round Round => Turn.Round;
        public Turn Turn { get; }
        public TurnPhase TurnPhase { get; }

        public TurnState(GamePhase gamePhase, Turn turn, TurnPhase turnPhase)
        {
            GamePhase = gamePhase;
            Turn = turn;
            TurnPhase = turnPhase;
        }
    }
}