using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Phases;

namespace Veggerby.Boards.Core.States
{
    public class TurnState : State<Player>
    {
        public Round Round => Turn.Round;
        public Turn Turn { get; }
        public GamePhase GamePhase { get; }
        public TurnPhase TurnPhase { get; }

        public TurnState(Player player, Turn turn, GamePhase gamePhase, TurnPhase turnPhase) : base(player)
        {
            Turn = turn;
            GamePhase = gamePhase;
            TurnPhase = turnPhase;
        }
    }
}