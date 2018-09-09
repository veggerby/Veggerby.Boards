using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class DoublingDiceState : DiceState<int>
    {
        public DoublingDiceState(Dice<int> dice, int currentValue, Player currentPlayer) : base(dice, currentValue)
        {
            CurrentPlayer = currentPlayer;
        }

        public Player CurrentPlayer { get; }
    }
}