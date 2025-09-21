using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// State for the Backgammon doubling cube including current value and owning player.
/// </summary>
public class DoublingDiceState(Dice dice, int currentValue, Player currentPlayer) : DiceState<int>(dice, currentValue)
{
    /// <summary>
    /// Gets the player currently in possession of the doubling cube.
    /// </summary>
    public Player CurrentPlayer { get; } = currentPlayer;
}