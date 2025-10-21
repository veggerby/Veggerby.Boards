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

        if (!tileRelations.Any())
        {
            throw new ArgumentException("Empty relations list", nameof(tileRelations));
        }

        TileRelations = tileRelations.ToList().AsReadOnly();
        Tiles = tileRelations.SelectMany(x => new[] { x.From, x.To }).Distinct().ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets a tile by identifier.
    /// </summary>
    /// <param name="tileId">Tile identifier.</param>
    /// <returns>The tile or null if not found.</returns>
    public Tile? GetTile(string tileId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tileId, nameof(tileId));

        return Tiles.SingleOrDefault(x => string.Equals(x.Id, tileId));
    }

    /// <summary>
    /// Gets a relation originating from a tile in a specific direction.
    /// </summary>
    public TileRelation? GetTileRelation(Tile from, Direction direction)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        ArgumentNullException.ThrowIfNull(direction, nameof(direction));

        return TileRelations.SingleOrDefault(x => x.From.Equals(from) && x.Direction.Equals(direction));
    }

    /// <summary>
    /// Gets a relation connecting two tiles.
    /// </summary>
    public TileRelation? GetTileRelation(Tile from, Tile to)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        ArgumentNullException.ThrowIfNull(to, nameof(to));

        return TileRelations.SingleOrDefault(x => x.From.Equals(from) && x.To.Equals(to));
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