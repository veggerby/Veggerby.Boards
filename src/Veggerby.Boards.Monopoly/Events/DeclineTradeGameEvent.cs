using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player declining a trade proposal.
/// </summary>
public class DeclineTradeGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player declining the trade (the target of the proposal).
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeclineTradeGameEvent"/> class.
    /// </summary>
    public DeclineTradeGameEvent(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
    }
}
