using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Algorithm.Graphs;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Relations
{
    public class ResolveTilePathDistanceVisitor : IPatternVisitor
    {
        public Board Board { get; }
        public Tile From { get; }
        public Tile To { get; }
        public int Distance { get; }
        public bool AllowOvershootDistance { get; }
        public TilePath ResultPath { get; private set; }

        public ResolveTilePathDistanceVisitor(Board board, Tile from, Tile to, int distance, bool allowOvershootDistance = false)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            if (distance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            Board = board;
            From = from;
            To = to;
            Distance = distance;
            AllowOvershootDistance = allowOvershootDistance;
        }

        public void Visit(MultiDirectionPattern pattern)
        {
            var paths = new List<TilePath>();
            foreach (var direction in pattern.Directions)
            {
                var path = GetPathFromDirection(direction, pattern.IsRepeatable);
                if (path != null)
                {
                    paths.Add(path);
                }
            }

            ResultPath = paths.Any() ? paths.OrderBy(x => x.Distance).First() : null;
        }

        public void Visit(NullPattern pattern)
        {
            ResultPath = null;
        }

        public void Visit(FixedPattern pattern)
        {
            var from = From;
            TilePath path = null;
            foreach (var direction in pattern.Pattern)
            {
                var relation = Board.GetTileRelation(from, direction);
                if (relation == null)
                {
                    ResultPath = null;
                    return;
                }

                path = TilePath.Create(path, relation);
                from = relation.To;
            }

            // path will always have a value because there will always be at least ONE direction in FixedPattern
            ResultPath = path.Distance == Distance ? path : null;
        }

        public void Visit(DirectionPattern pattern)
        {
            ResultPath = GetPathFromDirection(pattern.Direction, pattern.IsRepeatable);
        }

        public void Visit(AnyPattern pattern)
        {
            throw new NotImplementedException();
        }

        private TilePath GetPathFromDirection(Direction direction, bool isRepeatable)
        {
            var from = From;
            TilePath path = null;
            while (path == null || isRepeatable) // we have not yet taken a step or it is repeatable
            {
                var relation = Board.GetTileRelation(from, direction);

                if (relation == null)
                {
                    return null;
                }

                if (relation.To.Equals(From) || (path?.Tiles.Contains(relation.To) ?? false))
                {
                    // back to where we started or we crossed paths... break
                    return null;
                }

                path = TilePath.Create(path, relation);

                if (To.Equals(path.To))
                {
                    if (path.Distance > Distance)
                    {
                        return null;
                    }

                    if (path.Distance < Distance && !AllowOvershootDistance)
                    {
                        return null;
                    }

                    return path;
                }

                if (path.Distance == Distance)
                {
                    return path;
                }
                else if (path.Distance > Distance)
                {
                    return null;
                }

                from = relation.To;
            }

            return null;
        }
    }
}