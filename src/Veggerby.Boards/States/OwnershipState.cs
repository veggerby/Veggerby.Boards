using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Marker artifact for generic ownership state tracking.
/// </summary>
internal sealed class OwnershipStateMarker : Artifact
{
    private static readonly Dictionary<string, OwnershipStateMarker> _cache = new();

    private OwnershipStateMarker(string id) : base(id) { }

    /// <summary>
    /// Gets or creates a marker instance for a given context (e.g., "territory", "property").
    /// </summary>
    public static OwnershipStateMarker GetOrCreate(string context)
    {
        if (!_cache.TryGetValue(context, out var marker))
        {
            marker = new OwnershipStateMarker($"ownership-{context}");
            _cache[context] = marker;
        }

        return marker;
    }
}

/// <summary>
/// Generic ownership record for any owned item (territory, property, tile, etc.).
/// </summary>
/// <typeparam name="TKey">Type of the key identifying the owned item.</typeparam>
/// <remarks>
/// This type is internal to avoid exposing nullable strings in public API.
/// Use <see cref="OwnershipState{TKey}.GetOwner(TKey)"/> to query ownership.
/// </remarks>
internal sealed record Ownership<TKey> where TKey : notnull
{
    /// <summary>
    /// Gets the identifier of the owned item.
    /// </summary>
    public TKey Key
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
    /// Creates a new Ownership record.
    /// </summary>
    public Ownership(TKey key, string? ownerId = null)
    {
        Key = key;
        OwnerId = ownerId;
    }
}

/// <summary>
/// Generic state tracking ownership of multiple items by players.
/// </summary>
/// <typeparam name="TKey">Type of the key identifying owned items (e.g., Tile, string position, int index).</typeparam>
/// <remarks>
/// Provides a reusable foundation for ownership tracking across game modules:
/// - Risk: territory ownership
/// - Monopoly: property ownership  
/// - Go: territory influence (scoring)
/// 
/// The state is immutable; mutation methods return new instances.
/// </remarks>
public class OwnershipState<TKey> : IArtifactState where TKey : notnull
{
    private readonly Dictionary<TKey, string?> _ownership;
    private readonly string _context;

    /// <inheritdoc />
    public Artifact Artifact => OwnershipStateMarker.GetOrCreate(_context);

    /// <summary>
    /// Initializes a new instance with no ownership.
    /// </summary>
    /// <param name="context">Context identifier for this ownership type (e.g., "territory", "property").</param>
    public OwnershipState(string context)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(context);

        _context = context;
        _ownership = new Dictionary<TKey, string?>();
    }

    /// <summary>
    /// Initializes a new instance with existing ownership data.
    /// </summary>
    /// <param name="context">Context identifier for this ownership type.</param>
    /// <param name="ownership">Initial ownership mappings as (key, ownerId) tuples.</param>
    public OwnershipState(string context, IEnumerable<(TKey Key, string? OwnerId)> ownership)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(context);
        ArgumentNullException.ThrowIfNull(ownership);

        _context = context;
        _ownership = ownership.ToDictionary(o => o.Key, o => o.OwnerId);
    }

    private OwnershipState(string context, Dictionary<TKey, string?> ownership)
    {
        _context = context;
        _ownership = new Dictionary<TKey, string?>(ownership);
    }

    /// <summary>
    /// Gets the owner of an item.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <returns>The owner player ID, or null if unowned.</returns>
    public string? GetOwner(TKey key)
    {
        return _ownership.TryGetValue(key, out var ownerId) ? ownerId : null;
    }

    /// <summary>
    /// Gets whether an item is owned by any player.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <returns>True if owned; false otherwise.</returns>
    public bool IsOwned(TKey key)
    {
        return GetOwner(key) is not null;
    }

    /// <summary>
    /// Gets whether an item is owned by a specific player.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <param name="playerId">The player ID to check.</param>
    /// <returns>True if owned by the specified player; false otherwise.</returns>
    public bool IsOwnedBy(TKey key, string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        var owner = GetOwner(key);

        return owner is not null && string.Equals(owner, playerId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets all items owned by a specific player.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <returns>Enumerable of item keys owned by the player.</returns>
    public IEnumerable<TKey> GetOwnedBy(string playerId)
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
    /// Gets the count of items owned by a specific player.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <returns>Number of items owned.</returns>
    public int CountOwnedBy(string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);

        int count = 0;

        foreach (var kvp in _ownership)
        {
            if (string.Equals(kvp.Value, playerId, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets all tracked items as key-owner pairs.
    /// </summary>
    /// <returns>Enumerable of (key, ownerId) tuples.</returns>
    public IEnumerable<(TKey Key, string? OwnerId)> GetAll()
    {
        foreach (var kvp in _ownership)
        {
            yield return (kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Gets the total number of tracked items (owned or unowned).
    /// </summary>
    public int Count => _ownership.Count;

    /// <summary>
    /// Creates a new state with updated ownership for an item.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <param name="playerId">The new owner player ID (null to clear ownership).</param>
    /// <returns>A new OwnershipState instance with the updated ownership.</returns>
    public OwnershipState<TKey> SetOwner(TKey key, string? playerId)
    {
        var newOwnership = new Dictionary<TKey, string?>(_ownership)
        {
            [key] = playerId
        };

        return new OwnershipState<TKey>(_context, newOwnership);
    }

    /// <summary>
    /// Creates a new state with ownership transferred from one player to another for a specific item.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <param name="newOwnerId">The new owner player ID.</param>
    /// <returns>A new OwnershipState instance with the transferred ownership.</returns>
    public OwnershipState<TKey> TransferOwnership(TKey key, string newOwnerId)
    {
        ArgumentNullException.ThrowIfNull(newOwnerId);

        return SetOwner(key, newOwnerId);
    }

    /// <summary>
    /// Creates a new state with ownership cleared for an item.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <returns>A new OwnershipState instance with the ownership cleared.</returns>
    public OwnershipState<TKey> ClearOwner(TKey key)
    {
        return SetOwner(key, null);
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        if (other is not OwnershipState<TKey> oss)
        {
            return false;
        }

        if (!string.Equals(_context, oss._context, StringComparison.Ordinal))
        {
            return false;
        }

        if (_ownership.Count != oss._ownership.Count)
        {
            return false;
        }

        foreach (var kvp in _ownership)
        {
            if (!oss._ownership.TryGetValue(kvp.Key, out var otherOwner) ||
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
        return obj is OwnershipState<TKey> oss && Equals(oss);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(typeof(OwnershipState<TKey>));
        code.Add(_context);

        foreach (var kvp in _ownership.OrderBy(k => k.Key))
        {
            code.Add(kvp.Key);
            code.Add(kvp.Value);
        }

        return code.ToHashCode();
    }
}
