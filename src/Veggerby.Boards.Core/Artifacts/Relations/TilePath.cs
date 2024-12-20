using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts.Relations;

public class TilePath
{
    public IEnumerable<TileRelation> Relations { get; }
    public IEnumerable<Tile> Tiles => new[] { Relations.First().From }.Concat(Relations.Select(x => x.To)).ToList();
    public IEnumerable<Direction> Directions => Relations.Select(x => x.Direction).ToList();
    public Tile From => Relations.First().From;
    public Tile To => Relations.Last().To;
    public int Distance => Relations.Sum(x => x.Distance);

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

    public TilePath Add(TileRelation relation)
    {
        return new TilePath(Relations.Append(relation));
    }

    public static TilePath Create(TilePath path, TileRelation relation)
    {
        return path is not null ? path.Add(relation) : new TilePath([relation]);
    }

    public override string ToString()
    {
        var path = string.Join(" ", Relations.Select(x => $"{x.Direction} {x.To}"));
        return $"Path: {From} {path}";
    }
}