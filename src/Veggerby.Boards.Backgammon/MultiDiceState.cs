using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class MultiDiceState : DiceState<int[]>
    {
        public MultiDiceState(Dice dice, int[] currentValue) : base(dice, currentValue)
        {
        }
    }
}