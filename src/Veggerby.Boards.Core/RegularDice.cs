using System.Linq;

namespace Veggerby.Boards.Core
{
    public class RegularDice : Dice<int>
    {
        public RegularDice(string id, int count) 
            : base(id, Enumerable.Range(0, count).Select(x => new RegularDie($"{id}-{x}")))
        {
        }
    }
}