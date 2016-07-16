using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class MultiDirectionPattern : IPattern 
    {
        public IEnumerable<Direction> Directions { get; }
        public bool IsRepeatable { get; }

        public MultiDirectionPattern(IEnumerable<Direction> directions, bool isRepeatable = true)
        {
            Directions = (directions ?? Enumerable.Empty<Direction>()).ToList();
            IsRepeatable = isRepeatable;
        }

        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}