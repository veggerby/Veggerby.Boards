using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Risk.Events;

/// <summary>
/// Event representing placement of reinforcement armies on a territory.
/// </summary>
public sealed class PlaceArmiesGameEvent : IGameEvent
{
    private readonly Player _player;
    private readonly Tile _territory;
    private readonly int _armyCount;

    /// <summary>
    /// Gets the player placing armies.
    /// </summary>
    public Player Player => _player;

    /// <summary>
    /// Gets the target territory.
    /// </summary>
    public Tile Territory => _territory;

    /// <summary>
    /// Gets the number of armies to place.
    /// </summary>
    public int ArmyCount => _armyCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaceArmiesGameEvent"/> class.
    /// </summary>
    /// <param name="player">The player placing armies.</param>
    /// <param name="territory">The target territory.</param>
    /// <param name="armyCount">The number of armies to place (must be positive).</param>
    public PlaceArmiesGameEvent(Player player, Tile territory, int armyCount)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(territory, nameof(territory));

        if (armyCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(armyCount), "Must place at least one army.");
        }

        _player = player;
        _territory = territory;
        _armyCount = armyCount;
    }
}
