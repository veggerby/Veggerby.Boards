using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// State for the Backgammon doubling cube including current value, owning player, and turn on which it was last doubled.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DoublingDiceState"/> class.
/// </remarks>
/// <param name="dice">Doubling cube artifact.</param>
/// <param name="currentValue">Current cube face value.</param>
/// <param name="currentPlayer">Player currently in possession of the cube (may be <c>null</c> prior to first double).</param>
/// <param name="lastDoubledTurn">Turn number on which the cube was last doubled (0 if never).</param>
public class DoublingDiceState(Dice dice, int currentValue, Player currentPlayer, int lastDoubledTurn) : DiceState<int>(dice, currentValue)
{

    /// <summary>
    /// Gets the player currently in possession of the doubling cube.
    /// </summary>
    public Player CurrentPlayer { get; } = currentPlayer;

    /// <summary>
    /// Gets the last turn number on which the cube was doubled (0 if never).
    /// </summary>
    public int LastDoubledTurn { get; } = lastDoubledTurn;
}