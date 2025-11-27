using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player accepting a trade proposal.
/// </summary>
public class AcceptTradeGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player accepting the trade (the target of the proposal).
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptTradeGameEvent"/> class.
    /// </summary>
    public AcceptTradeGameEvent(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
    }
}
