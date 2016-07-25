using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Phases
{
    public class Round
    {
        public int Number { get; }
        public IEnumerable<Turn> Turns { get; }

        public Round(int number, IEnumerable<Turn> turns)
        {
            Number = number;
            Turns = (turns ?? Enumerable.Empty<Turn>()).ToList();
        }

        public Turn NextTurn(Turn turn)
        {
            if (turn == null)
            {
                return Turns.FirstOrDefault();
            }

            return Turns
                .SkipWhile(x => x != turn) // find the current turn 
                .Skip(1) // skip current turn
                .FirstOrDefault(); // take next
        }
    }
}