using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Immutable board artifact composed of tiles and directional relations between them.
/// </summary>
public class Board : Artifact, IEquatable<Board>
{
    private readonly Dictionary<string, Tile> _tileLookup;
    private readonly Dictionary<(Tile From, Tile To), TileRelation> _relationByFromTo;

    /// <summary>
    /// Gets all tiles belonging to the board.
    /// </summary>
    public IEnumerable<Tile> Tiles
    {
        get;
    }

    /// <summary>
    /// Gets all relations between tiles.
    /// </summary>
    public IEnumerable<TileRelation> TileRelations
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Board"/> class.
    /// </summary>
    /// <param name="id">Board identifier.</param>
    /// <param name="tileRelations">Relations connecting tiles that implicitly define the board's tile set.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tileRelations"/> is empty.</exception>
    public Board(string id, IEnumerable<TileRelation> tileRelations) : base(id)
    {
        ArgumentNullException.ThrowIfNull(tileRelations, nameof(tileRelations));

        var relationList = tileRelations.ToList();
        if (relationList.Count == 0)
        {
            throw new ArgumentException("Empty relations list", nameof(tileRelations));
        }

        TileRelations = relationList.AsReadOnly();

        // Extract unique tiles from relations
        var tileList = relationList.SelectMany(x => new[] { x.From, x.To }).Distinct().ToList();
        Tiles = tileList.AsReadOnly();

        // Build O(1) lookup dictionary for tile by id.
        // Validate uniqueness: duplicate IDs would have thrown InvalidOperationException
        // via SingleOrDefault in the original implementation.
        _tileLookup = new Dictionary<string, Tile>(tileList.Count, StringComparer.Ordinal);
        foreach (var tile in tileList)
        {
            if (_tileLookup.ContainsKey(tile.Id))
            {
                throw new InvalidOperationException($"Sequence contains more than one element with tile ID '{tile.Id}'");
            }

            _tileLookup[tile.Id] = tile;
        }

        // Build O(1) lookup dictionary for (From, To) relation pairs.
        // Validate uniqueness: duplicate (From, To) pairs would have thrown InvalidOperationException
        // via SingleOrDefault in the original implementation.
        _relationByFromTo = new Dictionary<(Tile, Tile), TileRelation>(relationList.Count);
        foreach (var relation in relationList)
        {
            var fromToKey = (relation.From, relation.To);
            if (_relationByFromTo.ContainsKey(fromToKey))
            {
                throw new InvalidOperationException(
                    $"Sequence contains more than one element with (From: '{relation.From.Id}', To: '{relation.To.Id}')");
            }

            _relationByFromTo[fromToKey] = relation;
        }
    }

    /// <summary>
    /// Gets a tile by identifier.
    /// </summary>
    /// <param name="tileId">Tile identifier.</param>
    /// <returns>The tile or null if not found.</returns>
    public Tile? GetTile(string tileId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tileId, nameof(tileId));

        return _tileLookup.TryGetValue(tileId, out var tile) ? tile : null;
    }

    /// <summary>
    /// Gets a relation originating from a tile in a specific direction.
    /// </summary>
    /// <remarks>
    /// Uses LINQ-based lookup because multiple relations can share the same (From, Direction) pair
    /// (e.g., adjacency in Risk-style boards). Returns the first match or null.
    /// </remarks>
    public TileRelation? GetTileRelation(Tile from, Direction direction)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        ArgumentNullException.ThrowIfNull(direction, nameof(direction));

        // Note: Using FirstOrDefault instead of SingleOrDefault because boards like Risk
        // can have multiple relations from the same tile in the same direction.
        return TileRelations.FirstOrDefault(x => x.From.Equals(from) && x.Direction.Equals(direction));
    }

    /// <summary>
    /// Gets a relation connecting two tiles.
    /// </summary>
    public TileRelation? GetTileRelation(Tile from, Tile to)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        ArgumentNullException.ThrowIfNull(to, nameof(to));

        return _relationByFromTo.TryGetValue((from, to), out var relation) ? relation : null;
    }

    /// <inheritdoc />
    public bool Equals(Board? other) => base.Equals(other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Board);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(Id);

        foreach (var tile in Tiles)
        {
            code.Add(tile);
        }

        foreach (var relation in TileRelations)
        {
            code.Add(relation);
        }

        return code.ToHashCode();
    }
}