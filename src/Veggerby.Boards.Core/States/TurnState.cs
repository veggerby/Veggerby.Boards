using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Phases;

namespace Veggerby.Boards.Core.States
{
    public class TurnState : State<Player>
    {
        public Round Round => Turn.Round;
        public Turn Turn { get; }

        public TurnState(Player player, Turn turn) : base(player)
        {
            Turn = turn;
        }
    }
}