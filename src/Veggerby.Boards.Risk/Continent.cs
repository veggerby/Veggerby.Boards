using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Represents a continent grouping of territories that provides bonus armies when fully controlled.
/// </summary>
/// <remarks>
/// Continents are immutable value objects that define a set of territory IDs and a bonus army value.
/// </remarks>
public sealed class Continent
{
    private readonly string _id;
    private readonly string _name;
    private readonly int _bonusArmies;
    private readonly IReadOnlyList<string> _territoryIds;

    /// <summary>
    /// Gets the unique identifier for this continent.
    /// </summary>
    public string Id => _id;

    /// <summary>
    /// Gets the display name of this continent.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Gets the number of bonus armies awarded for controlling all territories in this continent.
    /// </summary>
    public int BonusArmies => _bonusArmies;

    /// <summary>
    /// Gets the collection of territory IDs that belong to this continent.
    /// </summary>
    public IReadOnlyList<string> TerritoryIds => _territoryIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="Continent"/> class.
    /// </summary>
    /// <param name="id">Unique identifier.</param>
    /// <param name="name">Display name.</param>
    /// <param name="bonusArmies">Bonus armies for full control.</param>
    /// <param name="territoryIds">Territory IDs belonging to this continent.</param>
    public Continent(string id, string name, int bonusArmies, IReadOnlyList<string> territoryIds)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(id, nameof(id));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(territoryIds, nameof(territoryIds));

        if (bonusArmies < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bonusArmies), "Bonus armies cannot be negative.");
        }

        if (territoryIds.Count == 0)
        {
            throw new ArgumentException("A continent must contain at least one territory.", nameof(territoryIds));
        }

        _id = id;
        _name = name;
        _bonusArmies = bonusArmies;
        _territoryIds = territoryIds;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Continent other && string.Equals(_id, other._id, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(_id);
    }
}
