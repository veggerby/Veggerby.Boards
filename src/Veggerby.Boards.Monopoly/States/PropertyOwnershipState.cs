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
    /// Creates a new PropertyOwnership.
    /// </summary>
    public PropertyOwnership(int position, string? ownerId = null)
    {
        Position = position;
        OwnerId = ownerId;
    }
}

/// <summary>
/// Represents the ownership state of all properties on the board.
/// </summary>
public sealed class PropertyOwnershipState : IArtifactState
{
    private readonly Dictionary<int, string?> _ownership;

    /// <inheritdoc />
    public Artifact Artifact => PropertyOwnershipMarker.Instance;

    /// <summary>
    /// Initializes a new instance with no ownership.
    /// </summary>
    public PropertyOwnershipState()
    {
        _ownership = new Dictionary<int, string?>();
    }

    /// <summary>
    /// Initializes a new instance with existing ownership.
    /// </summary>
    public PropertyOwnershipState(IEnumerable<PropertyOwnership> ownership)
    {
        ArgumentNullException.ThrowIfNull(ownership);

        _ownership = ownership.ToDictionary(o => o.Position, o => o.OwnerId);
    }

    private PropertyOwnershipState(Dictionary<int, string?> ownership)
    {
        _ownership = new Dictionary<int, string?>(ownership);
    }

    /// <summary>
    /// Gets the owner of a property at the given position.
    /// </summary>
    public string? GetOwner(int position)
    {
        return _ownership.TryGetValue(position, out var owner) ? owner : null;
    }

    /// <summary>
    /// Gets whether a property is owned.
    /// </summary>
    public bool IsOwned(int position)
    {
        return GetOwner(position) is not null;
    }

    /// <summary>
    /// Gets all properties owned by a player.
    /// </summary>
    public IEnumerable<int> GetPropertiesOwnedBy(string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        foreach (var kvp in _ownership)
        {
            if (string.Equals(kvp.Value, playerId, StringComparison.Ordinal))
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
            if (_ownership.TryGetValue(square.Position, out var owner) &&
                string.Equals(owner, playerId, StringComparison.Ordinal))
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
    /// Creates a new state with updated ownership.
    /// </summary>
    public PropertyOwnershipState SetOwner(int position, string? playerId)
    {
        var newOwnership = new Dictionary<int, string?>(_ownership)
        {
            [position] = playerId
        };

        return new PropertyOwnershipState(newOwnership);
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
            if (!pos._ownership.TryGetValue(kvp.Key, out var otherOwner) ||
                !string.Equals(kvp.Value, otherOwner, StringComparison.Ordinal))
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
            code.Add(kvp.Value);
        }

        return code.ToHashCode();
    }
}
