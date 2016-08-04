using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Events
{
    public class EndTurnGameEvent : IGameEvent
    {
        public Player NextPlayer { get; }

        public EndTurnGameEvent(Player nextPlayer)
        {
            NextPlayer = nextPlayer;
        }
    }
}