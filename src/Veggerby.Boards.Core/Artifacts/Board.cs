using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts;

public class Board : Artifact, IEquatable<Board>
{
    public IEnumerable<Tile> Tiles { get; }
    public IEnumerable<TileRelation> TileRelations { get; }

    public Board(string id, IEnumerable<TileRelation> tileRelations) : base(id)
    {
        ArgumentNullException.ThrowIfNull(tileRelations);

        if (!tileRelations.Any())
        {
            throw new ArgumentException("Empty relations list", nameof(tileRelations));
        }

        TileRelations = tileRelations.ToList().AsReadOnly();
        Tiles = tileRelations.SelectMany(x => new[] { x.From, x.To }).Distinct().ToList().AsReadOnly();
    }

    public Tile GetTile(string tileId)
    {
        if (string.IsNullOrEmpty(tileId))
        {
            throw new ArgumentException("Invalid Tile Id", nameof(tileId));
        }

        return Tiles.SingleOrDefault(x => string.Equals(x.Id, tileId));
    }

    public TileRelation GetTileRelation(Tile from, Direction direction)
    {
        ArgumentNullException.ThrowIfNull(from);

        ArgumentNullException.ThrowIfNull(direction);

        return TileRelations.SingleOrDefault(x => x.From.Equals(from) && x.Direction.Equals(direction));
    }

    public TileRelation GetTileRelation(Tile from, Tile to)
    {
        ArgumentNullException.ThrowIfNull(from);

        ArgumentNullException.ThrowIfNull(to);

        return TileRelations.SingleOrDefault(x => x.From.Equals(from) && x.To.Equals(to));
    }

    public bool Equals(Board other) => base.Equals(other);

    public override bool Equals(object obj) => Equals(obj as Board);

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