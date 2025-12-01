using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player staying in jail for another turn.
/// </summary>
/// <remarks>
/// This event is used when a player in jail fails to roll doubles
/// and must stay in jail for another turn. The jail turn counter
/// is incremented accordingly.
/// </remarks>
public class StayInJailGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player staying in jail.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StayInJailGameEvent"/> class.
    /// </summary>
    /// <param name="player">The player staying in jail.</param>
    public StayInJailGameEvent(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
    }
}
