using System;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Rules
{
    public class ResolveTilePathPatternVisitor : IPatternVisitor
    {
        private readonly Board _board;
        private readonly Tile _from;
        private readonly Tile _to;
        private TilePath _resultPath;

        public ResolveTilePathPatternVisitor(Board board, Tile from, Tile to)
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

            if (from.Equals(to)) 
            {
                throw new ArgumentException(nameof(to));
            }

            _board = board;
            _from = from;
            _to = to;
        }

        public void Visit(MultiDirectionPattern pattern)
        {
            foreach (var direction in pattern.Directions)
            {
                var path = GetPathFromDirection(direction, pattern.IsRepeatable);
                if (path != null) 
                {
                    _resultPath = path;
                    return;
                }
            }
        }

        public void Visit(NullPattern pattern)
        {
            _resultPath = null;
        }

        public void Visit(FixedPattern pattern)
        {
            var from = _from;
            TilePath path = null;
            foreach (var direction in pattern.Pattern)
            {
                var relation = _board.GetTileRelation(_from, direction);
                if (relation == null) 
                {
                    _resultPath = null;
                    return;
                }

                path = TilePath.Create(path, relation);
            }

            _resultPath = _to.Equals(path?.To) ? path : null;
        }

        public void Visit(DirectionPattern pattern)
        {
            _resultPath = GetPathFromDirection(pattern.Direction, pattern.IsRepeatable);
        }

        public TilePath GetPathFromDirection(Direction direction, bool isRepeatable)
        {
            var from = _from;
            TilePath path = null;
            while (path == null || isRepeatable) // we have not yet taken a step or it is repeatable
            {
                var relation = _board.GetTileRelation(_from, direction);
                
                if (relation == null) 
                {
                    return null;
                }

                if (relation.To.Equals(_from) || path.Tiles.Contains(relation.To))
                {
                    // back to where we started or we crossed paths... break
                    return null;
                }

                path = TilePath.Create(path, relation);

                if (_to.Equals(path?.To))
                {
                    return path;
                }
            }

            return null;
        }
    }
}