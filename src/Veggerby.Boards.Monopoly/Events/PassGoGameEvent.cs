using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player passing Go and collecting $200.
/// </summary>
public class PassGoGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player passing Go.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PassGoGameEvent"/> class.
    /// </summary>
    public PassGoGameEvent(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
    }
}
