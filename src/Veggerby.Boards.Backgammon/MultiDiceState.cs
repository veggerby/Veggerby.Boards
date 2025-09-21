using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// State for a dice artifact holding multiple simultaneous values (standard Backgammon dice pair).
/// </summary>
public class MultiDiceState(Dice dice, int[] currentValue) : DiceState<int[]>(dice, currentValue)
{
}