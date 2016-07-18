using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Artifacts.Relations
{
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
            if (relations == null || !relations.Any() || relations.Any(x => x == null))
            {
                throw new ArgumentException(nameof(relations));
            }

            Relations = relations.ToList();

            if (Relations.Count() > 1)
            {
                var first = Relations.First();

                var chainedTo = Relations
                    .Skip(1)
                    .Aggregate(first.To, (to, relation) => to != null && relation.From.Equals(to) ? relation.To : null);

                if (chainedTo == null)
                {
                    throw new ArgumentException(nameof(relations), "Relations are not a connected");
                }
            }
        }

        public TilePath Add(TileRelation relation)
        {
            return new TilePath(Relations.Append(relation));
        }

        public static TilePath Create(TilePath path, TileRelation relation)
        {
            return path != null ? path.Add(relation) : new TilePath(new [] { relation });
        }

        public override string ToString()
        {
            var path = string.Join(" ", Relations.Select(x => $"{x.Direction} {x.To}"));
            return $"Path: {From} {path}";
        }
    }
}