using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.States;

/// <summary>
/// Marker artifact for property ownership state.
/// </summary>
internal sealed class PropertyOwnershipMarker : Artifact
{
    public static readonly PropertyOwnershipMarker Instance = new();

    private PropertyOwnershipMarker() : base("monopoly-property-ownership") { }
}

/// <summary>
/// Represents the ownership of a single property.
/// </summary>
public sealed record PropertyOwnership
{
    /// <summary>
    /// Gets the position of the property on the board.
    /// </summary>
    public int Position
    {
        get; init;
    }

    /// <summary>
    /// Gets the owner player ID (null if unowned).
    /// </summary>
    public string? OwnerId
    {
        get; init;
    }

    /// <summary>
    /// Gets the number of houses on this property (0-4, or 5 for hotel).
    /// </summary>
    public int HouseCount
    {
        get; init;
    }

    /// <summary>
    /// Gets whether the property is mortgaged.
    /// </summary>
    public bool IsMortgaged
    {
        get; init;
    }

    /// <summary>
    /// Creates a new PropertyOwnership.
    /// </summary>
    public PropertyOwnership(int position, string? ownerId = null, int houseCount = 0, bool isMortgaged = false)
    {
        Position = position;
        OwnerId = ownerId;
        HouseCount = houseCount;
        IsMortgaged = isMortgaged;
    }
}

/// <summary>
/// Represents the ownership state of all properties on the board.
/// </summary>
public sealed class PropertyOwnershipState : IArtifactState
{
    private readonly Dictionary<int, (string? OwnerId, int HouseCount, bool IsMortgaged)> _ownership;

    /// <summary>
    /// Maximum number of houses per property (before hotel).
    /// </summary>
    public const int MaxHouses = 4;

    /// <summary>
    /// House count value representing a hotel.
    /// </summary>
    public const int HotelValue = 5;

    /// <inheritdoc />
    public Artifact Artifact => PropertyOwnershipMarker.Instance;

    /// <summary>
    /// Initializes a new instance with no ownership.
    /// </summary>
    public PropertyOwnershipState()
    {
        _ownership = new Dictionary<int, (string? OwnerId, int HouseCount, bool IsMortgaged)>();
    }

    /// <summary>
    /// Initializes a new instance with existing ownership.
    /// </summary>
    public PropertyOwnershipState(IEnumerable<PropertyOwnership> ownership)
    {
        ArgumentNullException.ThrowIfNull(ownership);

        _ownership = ownership.ToDictionary(o => o.Position, o => (o.OwnerId, o.HouseCount, o.IsMortgaged));
    }

    private PropertyOwnershipState(Dictionary<int, (string? OwnerId, int HouseCount, bool IsMortgaged)> ownership)
    {
        _ownership = new Dictionary<int, (string? OwnerId, int HouseCount, bool IsMortgaged)>(ownership);
    }

    /// <summary>
    /// Gets the owner of a property at the given position.
    /// </summary>
    public string? GetOwner(int position)
    {
        return _ownership.TryGetValue(position, out var data) ? data.OwnerId : null;
    }

    /// <summary>
    /// Gets whether a property is owned.
    /// </summary>
    public bool IsOwned(int position)
    {
        return GetOwner(position) is not null;
    }

    /// <summary>
    /// Gets the number of houses on a property (0-4, or 5 for hotel).
    /// </summary>
    public int GetHouseCount(int position)
    {
        return _ownership.TryGetValue(position, out var data) ? data.HouseCount : 0;
    }

    /// <summary>
    /// Gets whether a property has a hotel.
    /// </summary>
    public bool HasHotel(int position)
    {
        return GetHouseCount(position) == HotelValue;
    }

    /// <summary>
    /// Gets whether a property is mortgaged.
    /// </summary>
    public bool IsMortgaged(int position)
    {
        return _ownership.TryGetValue(position, out var data) && data.IsMortgaged;
    }

    /// <summary>
    /// Gets all properties owned by a player.
    /// </summary>
    public IEnumerable<int> GetPropertiesOwnedBy(string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        foreach (var kvp in _ownership)
        {
            if (string.Equals(kvp.Value.OwnerId, playerId, StringComparison.Ordinal))
            {
                yield return kvp.Key;
            }
        }
    }

    /// <summary>
    /// Gets the count of properties in a color group owned by a player.
    /// </summary>
    public int CountOwnedInColorGroup(string playerId, PropertyColorGroup colorGroup)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        if (!MonopolyBoardConfiguration.SquaresByColorGroup.TryGetValue(colorGroup, out var groupSquares))
        {
            return 0;
        }

        int count = 0;
        foreach (var square in groupSquares)
        {
            if (_ownership.TryGetValue(square.Position, out var data) &&
                string.Equals(data.OwnerId, playerId, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the count of unmortgaged properties in a color group owned by a player.
    /// </summary>
    public int CountUnmortgagedInColorGroup(string playerId, PropertyColorGroup colorGroup)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        if (!MonopolyBoardConfiguration.SquaresByColorGroup.TryGetValue(colorGroup, out var groupSquares))
        {
            return 0;
        }

        int count = 0;
        foreach (var square in groupSquares)
        {
            if (_ownership.TryGetValue(square.Position, out var data) &&
                string.Equals(data.OwnerId, playerId, StringComparison.Ordinal) &&
                !data.IsMortgaged)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets whether a player has a monopoly (owns all properties in a color group).
    /// </summary>
    public bool HasMonopoly(string playerId, PropertyColorGroup colorGroup)
    {
        var totalInGroup = MonopolyBoardConfiguration.GetColorGroupCount(colorGroup);

        return totalInGroup > 0 && CountOwnedInColorGroup(playerId, colorGroup) == totalInGroup;
    }

    /// <summary>
    /// Gets whether a player has an active (unmortgaged) monopoly.
    /// </summary>
    public bool HasActiveMonopoly(string playerId, PropertyColorGroup colorGroup)
    {
        if (!HasMonopoly(playerId, colorGroup))
        {
            return false;
        }

        // All properties in the group must be unmortgaged for an active monopoly
        if (!MonopolyBoardConfiguration.SquaresByColorGroup.TryGetValue(colorGroup, out var groupSquares))
        {
            return false;
        }

        foreach (var square in groupSquares)
        {
            if (_ownership.TryGetValue(square.Position, out var data) && data.IsMortgaged)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the minimum house count across all properties in a color group for a player.
    /// </summary>
    public int GetMinHouseCountInColorGroup(string playerId, PropertyColorGroup colorGroup)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        if (!MonopolyBoardConfiguration.SquaresByColorGroup.TryGetValue(colorGroup, out var groupSquares))
        {
            return 0;
        }

        int minCount = int.MaxValue;
        bool hasAny = false;

        foreach (var square in groupSquares)
        {
            if (_ownership.TryGetValue(square.Position, out var data) &&
                string.Equals(data.OwnerId, playerId, StringComparison.Ordinal))
            {
                hasAny = true;
                if (data.HouseCount < minCount)
                {
                    minCount = data.HouseCount;
                }
            }
        }

        return hasAny ? minCount : 0;
    }

    /// <summary>
    /// Gets the maximum house count across all properties in a color group for a player.
    /// </summary>
    public int GetMaxHouseCountInColorGroup(string playerId, PropertyColorGroup colorGroup)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        if (!MonopolyBoardConfiguration.SquaresByColorGroup.TryGetValue(colorGroup, out var groupSquares))
        {
            return 0;
        }

        int maxCount = 0;

        foreach (var square in groupSquares)
        {
            if (_ownership.TryGetValue(square.Position, out var data) &&
                string.Equals(data.OwnerId, playerId, StringComparison.Ordinal))
            {
                if (data.HouseCount > maxCount)
                {
                    maxCount = data.HouseCount;
                }
            }
        }

        return maxCount;
    }

    /// <summary>
    /// Checks if a house can be built on a property (even building rule).
    /// </summary>
    public bool CanBuildHouse(int position, string playerId, PropertyColorGroup colorGroup)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        // Must have monopoly
        if (!HasMonopoly(playerId, colorGroup))
        {
            return false;
        }

        // Can't build on railroads or utilities
        if (colorGroup == PropertyColorGroup.Railroad || colorGroup == PropertyColorGroup.Utility)
        {
            return false;
        }

        // Can't build on mortgaged property
        if (IsMortgaged(position))
        {
            return false;
        }

        // All properties in the color group must be unmortgaged to build
        if (!HasActiveMonopoly(playerId, colorGroup))
        {
            return false;
        }

        var currentHouses = GetHouseCount(position);

        // Already has a hotel
        if (currentHouses >= HotelValue)
        {
            return false;
        }

        // Already has max houses (needs hotel upgrade)
        if (currentHouses >= MaxHouses)
        {
            return false;
        }

        // Even building rule: can only build if this property has the minimum houses in the group
        var minHouses = GetMinHouseCountInColorGroup(playerId, colorGroup);

        return currentHouses == minHouses;
    }

    /// <summary>
    /// Checks if a hotel can be built on a property.
    /// </summary>
    public bool CanBuildHotel(int position, string playerId, PropertyColorGroup colorGroup)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        // Must have monopoly
        if (!HasMonopoly(playerId, colorGroup))
        {
            return false;
        }

        // Can't build on railroads or utilities
        if (colorGroup == PropertyColorGroup.Railroad || colorGroup == PropertyColorGroup.Utility)
        {
            return false;
        }

        // Can't build on mortgaged property
        if (IsMortgaged(position))
        {
            return false;
        }

        // All properties in the color group must be unmortgaged to build
        if (!HasActiveMonopoly(playerId, colorGroup))
        {
            return false;
        }

        var currentHouses = GetHouseCount(position);

        // Must have exactly 4 houses to build hotel
        if (currentHouses != MaxHouses)
        {
            return false;
        }

        // Even building rule: all properties in group must have 4 houses or a hotel
        var minHouses = GetMinHouseCountInColorGroup(playerId, colorGroup);

        return minHouses >= MaxHouses;
    }

    /// <summary>
    /// Checks if a property can be mortgaged.
    /// </summary>
    public bool CanMortgage(int position, string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        // Must own the property
        if (!_ownership.TryGetValue(position, out var data))
        {
            return false;
        }

        if (!string.Equals(data.OwnerId, playerId, StringComparison.Ordinal))
        {
            return false;
        }

        // Already mortgaged
        if (data.IsMortgaged)
        {
            return false;
        }

        // Cannot mortgage if there are houses on this property
        if (data.HouseCount > 0)
        {
            return false;
        }

        // Cannot mortgage if any property in the color group has houses
        var squareInfo = MonopolyBoardConfiguration.GetSquare(position);
        if (squareInfo.ColorGroup != PropertyColorGroup.None &&
            squareInfo.ColorGroup != PropertyColorGroup.Railroad &&
            squareInfo.ColorGroup != PropertyColorGroup.Utility)
        {
            var maxHouses = GetMaxHouseCountInColorGroup(playerId, squareInfo.ColorGroup);
            if (maxHouses > 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a property can be unmortgaged.
    /// </summary>
    public bool CanUnmortgage(int position, string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        // Must own the property
        if (!_ownership.TryGetValue(position, out var data))
        {
            return false;
        }

        if (!string.Equals(data.OwnerId, playerId, StringComparison.Ordinal))
        {
            return false;
        }

        // Must be mortgaged to unmortgage
        return data.IsMortgaged;
    }

    /// <summary>
    /// Creates a new state with updated ownership.
    /// </summary>
    public PropertyOwnershipState SetOwner(int position, string? playerId)
    {
        var newOwnership = new Dictionary<int, (string? OwnerId, int HouseCount, bool IsMortgaged)>(_ownership);
        var (_, houseCount, isMortgaged) = _ownership.TryGetValue(position, out var existing) ? existing : (null, 0, false);
        newOwnership[position] = (playerId, houseCount, isMortgaged);

        return new PropertyOwnershipState(newOwnership);
    }

    /// <summary>
    /// Creates a new state with updated house count.
    /// </summary>
    public PropertyOwnershipState SetHouseCount(int position, int houseCount)
    {
        if (houseCount < 0 || houseCount > HotelValue)
        {
            throw new ArgumentOutOfRangeException(nameof(houseCount), "House count must be between 0 and 5");
        }

        var newOwnership = new Dictionary<int, (string? OwnerId, int HouseCount, bool IsMortgaged)>(_ownership);
        var (owner, _, isMortgaged) = _ownership.TryGetValue(position, out var existing) ? existing : (null, 0, false);
        newOwnership[position] = (owner, houseCount, isMortgaged);

        return new PropertyOwnershipState(newOwnership);
    }

    /// <summary>
    /// Creates a new state with one additional house (or hotel if at 4 houses).
    /// </summary>
    public PropertyOwnershipState AddHouse(int position)
    {
        var currentCount = GetHouseCount(position);

        if (currentCount >= HotelValue)
        {
            throw new InvalidOperationException("Cannot add more houses - property already has a hotel");
        }

        return SetHouseCount(position, currentCount + 1);
    }

    /// <summary>
    /// Creates a new state with updated mortgage status.
    /// </summary>
    public PropertyOwnershipState SetMortgaged(int position, bool isMortgaged)
    {
        var newOwnership = new Dictionary<int, (string? OwnerId, int HouseCount, bool IsMortgaged)>(_ownership);
        var (owner, houseCount, _) = _ownership.TryGetValue(position, out var existing) ? existing : (null, 0, false);
        newOwnership[position] = (owner, houseCount, isMortgaged);

        return new PropertyOwnershipState(newOwnership);
    }

    /// <summary>
    /// Creates a new state with the property mortgaged.
    /// </summary>
    public PropertyOwnershipState Mortgage(int position)
    {
        return SetMortgaged(position, true);
    }

    /// <summary>
    /// Creates a new state with the property unmortgaged.
    /// </summary>
    public PropertyOwnershipState Unmortgage(int position)
    {
        return SetMortgaged(position, false);
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        if (other is not PropertyOwnershipState pos)
        {
            return false;
        }

        if (_ownership.Count != pos._ownership.Count)
        {
            return false;
        }

        foreach (var kvp in _ownership)
        {
            if (!pos._ownership.TryGetValue(kvp.Key, out var otherData) ||
                !string.Equals(kvp.Value.OwnerId, otherData.OwnerId, StringComparison.Ordinal) ||
                kvp.Value.HouseCount != otherData.HouseCount ||
                kvp.Value.IsMortgaged != otherData.IsMortgaged)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is PropertyOwnershipState pos && Equals(pos);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(typeof(PropertyOwnershipState));

        foreach (var kvp in _ownership.OrderBy(k => k.Key))
        {
            code.Add(kvp.Key);
            code.Add(kvp.Value.OwnerId);
            code.Add(kvp.Value.HouseCount);
            code.Add(kvp.Value.IsMortgaged);
        }

        return code.ToHashCode();
    }
}
