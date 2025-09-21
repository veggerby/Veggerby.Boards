using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// State for a dice artifact holding multiple simultaneous values (standard Backgammon dice pair).
/// </summary>
public class MultiDiceState(Dice dice, int[] currentValue) : DiceState<int[]>(dice, currentValue)
{
}