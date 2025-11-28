using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Risk.Events;

/// <summary>
/// Event representing the capture of a territory after reducing defender armies to zero.
/// </summary>
public sealed class ConquerTerritoryGameEvent : IGameEvent
{
    private readonly Tile _territory;
    private readonly Player _newOwner;
    private readonly int _movingArmies;
    private readonly Tile _fromTerritory;

    /// <summary>
    /// Gets the conquered territory.
    /// </summary>
    public Tile Territory => _territory;

    /// <summary>
    /// Gets the new owner (attacker).
    /// </summary>
    public Player NewOwner => _newOwner;

    /// <summary>
    /// Gets the number of armies moving into the conquered territory.
    /// </summary>
    public int MovingArmies => _movingArmies;

    /// <summary>
    /// Gets the source territory from which armies are moving.
    /// </summary>
    public Tile FromTerritory => _fromTerritory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConquerTerritoryGameEvent"/> class.
    /// </summary>
    /// <param name="territory">The conquered territory.</param>
    /// <param name="newOwner">The new owner.</param>
    /// <param name="movingArmies">The number of armies to move in.</param>
    /// <param name="fromTerritory">The source territory.</param>
    public ConquerTerritoryGameEvent(Tile territory, Player newOwner, int movingArmies, Tile fromTerritory)
    {
        ArgumentNullException.ThrowIfNull(territory, nameof(territory));
        ArgumentNullException.ThrowIfNull(newOwner, nameof(newOwner));
        ArgumentNullException.ThrowIfNull(fromTerritory, nameof(fromTerritory));

        if (movingArmies < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(movingArmies), "Must move at least 1 army into conquered territory.");
        }

        _territory = territory;
        _newOwner = newOwner;
        _movingArmies = movingArmies;
        _fromTerritory = fromTerritory;
    }
}
