using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Represents the state of a territory including ownership and army count.
/// </summary>
/// <remarks>
/// TerritoryState is immutable; changes produce new instances via GameState.Next().
/// </remarks>
public sealed class TerritoryState : IArtifactState
{
    private readonly Tile _territory;
    private readonly Player _owner;
    private readonly int _armyCount;

    /// <summary>
    /// Gets the territory (tile) this state belongs to.
    /// </summary>
    public Tile Territory => _territory;

    /// <summary>
    /// Gets the player who owns this territory.
    /// </summary>
    public Player Owner => _owner;

    /// <summary>
    /// Gets the number of armies stationed on this territory.
    /// </summary>
    public int ArmyCount => _armyCount;

    /// <inheritdoc />
    public Artifact Artifact => _territory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerritoryState"/> class.
    /// </summary>
    /// <param name="territory">The territory tile.</param>
    /// <param name="owner">The owning player.</param>
    /// <param name="armyCount">The number of armies (must be at least 1 for owned territory).</param>
    public TerritoryState(Tile territory, Player owner, int armyCount)
    {
        ArgumentNullException.ThrowIfNull(territory, nameof(territory));
        ArgumentNullException.ThrowIfNull(owner, nameof(owner));

        if (armyCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(armyCount), "Army count must be at least 1.");
        }

        _territory = territory;
        _owner = owner;
        _armyCount = armyCount;
    }

    /// <summary>
    /// Creates a new TerritoryState with an updated army count.
    /// </summary>
    /// <param name="delta">The change in army count (positive or negative).</param>
    /// <returns>A new TerritoryState with the updated army count.</returns>
    public TerritoryState WithArmyDelta(int delta)
    {
        var newCount = _armyCount + delta;

        if (newCount < 1)
        {
            throw new InvalidOperationException($"Cannot reduce army count below 1. Current: {_armyCount}, Delta: {delta}");
        }

        return new TerritoryState(_territory, _owner, newCount);
    }

    /// <summary>
    /// Creates a new TerritoryState with a new owner (conquest).
    /// </summary>
    /// <param name="newOwner">The new owning player.</param>
    /// <param name="armyCount">The army count for the new owner.</param>
    /// <returns>A new TerritoryState with the new owner.</returns>
    public TerritoryState WithNewOwner(Player newOwner, int armyCount)
    {
        ArgumentNullException.ThrowIfNull(newOwner, nameof(newOwner));

        if (armyCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(armyCount), "Army count must be at least 1.");
        }

        return new TerritoryState(_territory, newOwner, armyCount);
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is TerritoryState ts &&
               Equals(ts._territory, _territory) &&
               Equals(ts._owner, _owner) &&
               ts._armyCount == _armyCount;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is IArtifactState state && Equals(state);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_territory, _owner, _armyCount);
    }
}
