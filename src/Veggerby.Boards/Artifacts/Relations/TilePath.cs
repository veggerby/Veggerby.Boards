using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Represents a connected sequence of <see cref="TileRelation"/> instances.
/// </summary>
public class TilePath
{
    private readonly TileRelation[] _relations; // dense array for fast iteration
    private readonly Tile[] _tiles; // cached tiles (from + each relation.To)
    private readonly Direction[] _directions; // cached directions sequence
    private readonly int _distance; // precomputed distance sum

    /// <summary>
    /// Gets the ordered relations comprising the path.
    /// </summary>
    public IReadOnlyList<TileRelation> Relations => _relations;

    /// <summary>
    /// Gets all tiles on the path including start and end.
    /// </summary>
    public IReadOnlyList<Tile> Tiles => _tiles;

    /// <summary>
    /// Gets the sequence of directions traversed.
    /// </summary>
    public IReadOnlyList<Direction> Directions => _directions;

    /// <summary>
    /// Gets the origin tile.
    /// </summary>
    public Tile From => _relations[0].From;

    /// <summary>
    /// Gets the destination tile.
    /// </summary>
    public Tile To => _relations[_relations.Length - 1].To;

    /// <summary>
    /// Gets the total distance (sum of relation distances).
    /// </summary>
    public int Distance => _distance;

    /// <summary>
    /// Initializes a new path from a set of connected relations.
    /// </summary>
    /// <param name="relations">Ordered, connected relations.</param>
    /// <exception cref="ArgumentException">Thrown when relations are null, empty, contain null elements, or not connected.</exception>
    public TilePath(IEnumerable<TileRelation> relations)
    {
        if (relations is null)
        {
            throw new ArgumentException("Invalid relations", nameof(relations));
        }

        // Materialize once into list for validation then array for storage.
        var list = relations as IList<TileRelation> ?? relations.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("Invalid relations", nameof(relations));
        }

        // Validate (no nulls)
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is null)
            {
                throw new ArgumentException("Invalid relations", nameof(relations));
            }
        }

        // Connectivity validation
        if (list.Count > 1)
        {
            var prevTo = list[0].To;
            for (int i = 1; i < list.Count; i++)
            {
                if (!list[i].From.Equals(prevTo))
                {
                    throw new ArgumentException("Relations are not a connected", nameof(relations));
                }

                prevTo = list[i].To;
            }
        }

        _relations = list is List<TileRelation> tl ? tl.ToArray() : list.ToArray();

        // Cache tiles
        _tiles = new Tile[_relations.Length + 1];
        _tiles[0] = _relations[0].From;
        for (int i = 0; i < _relations.Length; i++)
        {
            _tiles[i + 1] = _relations[i].To;
        }

        // Cache directions & distance
        _directions = new Direction[_relations.Length];
        var dist = 0;
        for (int i = 0; i < _relations.Length; i++)
        {
            var r = _relations[i];
            _directions[i] = r.Direction;
            dist += r.Distance;
        }

        _distance = dist;
    }

    /// <summary>
    /// Appends a relation to create a new path instance.
    /// </summary>
    public TilePath Add(TileRelation relation)
    {
        if (relation is null)
        {
            throw new ArgumentNullException(nameof(relation));
        }

        // Validate connectivity with last relation To tile.
        var lastTo = _relations[_relations.Length - 1].To;
        if (!relation.From.Equals(lastTo))
        {
            throw new ArgumentException("Relation does not connect to path", nameof(relation));
        }

        // Fast append path: build new array (allocation proportional to length; acceptable given path construction is not inner-loop hot).
        var newRelations = new TileRelation[_relations.Length + 1];
        Array.Copy(_relations, newRelations, _relations.Length);
        newRelations[_relations.Length] = relation;
        return new TilePath(newRelations);
    }

    /// <summary>
    /// Helper for adding or creating a path in a single call.
    /// </summary>
    public static TilePath Create(TilePath? path, TileRelation relation)
    {
        return path is not null ? path.Add(relation) : new TilePath(new[] { relation });
    }

    /// <inheritdoc />
    public override string ToString()
    {
        // Build descriptive string without LINQ for consistency.
        if (_relations.Length == 0)
        {
            return "Path: (empty)";
        }

        var sb = new System.Text.StringBuilder();
        sb.Append("Path: ").Append(From);
        for (int i = 0; i < _relations.Length; i++)
        {
            var r = _relations[i];
            sb.Append(' ').Append(r.Direction).Append(' ').Append(r.To);
        }

        return sb.ToString();
    }
}