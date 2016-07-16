using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class FixedPattern : IPattern 
    {
        public IEnumerable<Direction> Pattern { get; }

        public FixedPattern(IEnumerable<Direction> pattern)
        {
            Pattern = (pattern ?? Enumerable.Empty<Direction>()).ToList();
        }

        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}