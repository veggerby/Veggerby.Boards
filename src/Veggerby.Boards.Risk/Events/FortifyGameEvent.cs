using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Risk.Events;

/// <summary>
/// Event representing end-of-turn fortification movement between owned connected territories.
/// </summary>
public sealed class FortifyGameEvent : IGameEvent
{
    private readonly Tile _fromTerritory;
    private readonly Tile _toTerritory;
    private readonly int _armyCount;

    /// <summary>
    /// Gets the source territory (must retain at least 1 army).
    /// </summary>
    public Tile FromTerritory => _fromTerritory;

    /// <summary>
    /// Gets the destination territory.
    /// </summary>
    public Tile ToTerritory => _toTerritory;

    /// <summary>
    /// Gets the number of armies to move.
    /// </summary>
    public int ArmyCount => _armyCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="FortifyGameEvent"/> class.
    /// </summary>
    /// <param name="fromTerritory">The source territory.</param>
    /// <param name="toTerritory">The destination territory.</param>
    /// <param name="armyCount">The number of armies to move.</param>
    public FortifyGameEvent(Tile fromTerritory, Tile toTerritory, int armyCount)
    {
        ArgumentNullException.ThrowIfNull(fromTerritory, nameof(fromTerritory));
        ArgumentNullException.ThrowIfNull(toTerritory, nameof(toTerritory));

        if (armyCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(armyCount), "Must move at least 1 army.");
        }

        _fromTerritory = fromTerritory;
        _toTerritory = toTerritory;
        _armyCount = armyCount;
    }
}
