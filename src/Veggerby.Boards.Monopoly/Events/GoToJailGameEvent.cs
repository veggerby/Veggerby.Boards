using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player going to jail.
/// </summary>
public class GoToJailGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player going to jail.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoToJailGameEvent"/> class.
    /// </summary>
    public GoToJailGameEvent(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
    }
}
