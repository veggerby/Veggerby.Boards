using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon;

public class DoublingDiceState(Dice dice, int currentValue, Player currentPlayer) : DiceState<int>(dice, currentValue)
{
    public Player CurrentPlayer { get; } = currentPlayer;
}