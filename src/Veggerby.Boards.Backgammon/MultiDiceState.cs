using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon;

public class MultiDiceState(Dice dice, int[] currentValue) : DiceState<int[]>(dice, currentValue)
{
}