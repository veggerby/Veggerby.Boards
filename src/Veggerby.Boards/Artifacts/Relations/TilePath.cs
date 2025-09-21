using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Represents a connected sequence of <see cref="TileRelation"/> instances.
/// </summary>
public class TilePath
{
    /// <summary>
    /// Gets the ordered relations comprising the path.
    /// </summary>
    public IEnumerable<TileRelation> Relations { get; }
    /// <summary>
    /// Gets all tiles on the path including start and end.
    /// </summary>
    public IEnumerable<Tile> Tiles => [Relations.First().From, .. Relations.Select(x => x.To)];
    /// <summary>
    /// Gets the sequence of directions traversed.
    /// </summary>
    public IEnumerable<Direction> Directions => [.. Relations.Select(x => x.Direction)];
    /// <summary>
    /// Gets the origin tile.
    /// </summary>
    public Tile From => Relations.First().From;
    /// <summary>
    /// Gets the destination tile.
    /// </summary>
    public Tile To => Relations.Last().To;
    /// <summary>
    /// Gets the total distance (sum of relation distances).
    /// </summary>
    public int Distance => Relations.Sum(x => x.Distance);

    /// <summary>
    /// Initializes a new path from a set of connected relations.
    /// </summary>
    /// <param name="relations">Ordered, connected relations.</param>
    /// <exception cref="ArgumentException">Thrown when relations are null, empty, contain null elements, or not connected.</exception>
    public TilePath(IEnumerable<TileRelation> relations)
    {
        if (relations is null || !relations.Any() || relations.Any(x => x is null))
        {
            throw new ArgumentException("Invalid relations", nameof(relations));
        }

        Relations = relations.ToList().AsReadOnly();

        if (Relations.Count() > 1)
        {
            var first = Relations.First();

            var chainedTo = Relations
                .Skip(1)
                .Aggregate(first.To, (to, relation) => to is not null && relation.From.Equals(to) ? relation.To : null);

            if (chainedTo is null)
            {
                throw new ArgumentException("Relations are not a connected", nameof(relations));
            }
        }
    }

    /// <summary>
    /// Appends a relation to create a new path instance.
    /// </summary>
    public TilePath Add(TileRelation relation)
    {
        return new TilePath(Relations.Append(relation));
    }

    /// <summary>
    /// Helper for adding or creating a path in a single call.
    /// </summary>
    public static TilePath Create(TilePath path, TileRelation relation)
    {
        return path is not null ? path.Add(relation) : new TilePath([relation]);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var path = string.Join(" ", Relations.Select(x => $"{x.Direction} {x.To}"));
        return $"Path: {From} {path}";
    }
}