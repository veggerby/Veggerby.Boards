using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player moving on the board.
/// </summary>
public class MovePlayerGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player moving.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the number of spaces to move.
    /// </summary>
    public int Spaces
    {
        get;
    }

    /// <summary>
    /// Gets the first die value.
    /// </summary>
    public int Die1
    {
        get;
    }

    /// <summary>
    /// Gets the second die value.
    /// </summary>
    public int Die2
    {
        get;
    }

    /// <summary>
    /// Gets whether doubles were rolled.
    /// </summary>
    public bool IsDoubles => Die1 == Die2;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlayerGameEvent"/> class.
    /// </summary>
    public MovePlayerGameEvent(Player player, int die1, int die2)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (die1 < 1 || die1 > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(die1), "Die value must be between 1 and 6");
        }

        if (die2 < 1 || die2 > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(die2), "Die value must be between 1 and 6");
        }

        Player = player;
        Die1 = die1;
        Die2 = die2;
        Spaces = die1 + die2;
    }
}
