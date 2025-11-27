using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player moving to a specific position on the board.
/// This is used for card effects like "Advance to Go" or "Go to Jail".
/// </summary>
public class MoveToPositionGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player moving.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the target position on the board.
    /// </summary>
    public int TargetPosition
    {
        get;
    }

    /// <summary>
    /// Gets whether the player should collect $200 if passing Go.
    /// </summary>
    public bool CollectGoIfPassing
    {
        get;
    }

    /// <summary>
    /// Gets the source of the movement (e.g., "Chance card", "Community Chest").
    /// </summary>
    public string Source
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveToPositionGameEvent"/> class.
    /// </summary>
    public MoveToPositionGameEvent(Player player, int targetPosition, bool collectGoIfPassing = true, string source = "")
    {
        ArgumentNullException.ThrowIfNull(player);

        if (targetPosition < 0 || targetPosition >= 40)
        {
            throw new ArgumentOutOfRangeException(nameof(targetPosition), "Target position must be between 0 and 39");
        }

        Player = player;
        TargetPosition = targetPosition;
        CollectGoIfPassing = collectGoIfPassing;
        Source = source ?? "";
    }
}
