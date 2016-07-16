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
        public Tile To => Relations.First().To;
        public int Distance => Relations.Sum(x => x.Distance);

        public TilePath(IEnumerable<TileRelation> relations)
        {
            if (relations == null || !relations.Any())
            {
                throw new ArgumentException(nameof(relations));
            }

            Relations = relations.ToList();
        }

        public TilePath Add(TileRelation relation)
        {
            return new TilePath(Relations.Append(relation));
        }

        public static TilePath Create(TilePath path, TileRelation relation)
        {
            return path != null ? path.Add(relation) : new TilePath(new [] { relation });
        }
    }
}