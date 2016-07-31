using System;
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
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            if (!pattern.Any())
            {
                throw new ArgumentException("Empty pattern list", nameof(pattern));
            }

            Pattern = pattern.ToList();
        }

        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}