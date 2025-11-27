using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing a player being eliminated (bankrupt).
/// </summary>
public class EliminatePlayerGameEvent : IGameEvent
{
    /// <summary>
    /// Gets the player being eliminated.
    /// </summary>
    public Player Player
    {
        get;
    }

    /// <summary>
    /// Gets the player who bankrupted this player (null if bankrupted by bank).
    /// </summary>
    public Player? BankruptedBy
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EliminatePlayerGameEvent"/> class.
    /// </summary>
    public EliminatePlayerGameEvent(Player player, Player? bankruptedBy = null)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        BankruptedBy = bankruptedBy;
    }
}
