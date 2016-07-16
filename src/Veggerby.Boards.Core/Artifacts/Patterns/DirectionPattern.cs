using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class DirectionPattern : IPattern 
    {
        public Direction Direction { get; }
        public bool IsRepeatable { get; }

        public DirectionPattern(Direction direction, bool isRepeatable = true)
        {
            Direction = direction;
            IsRepeatable = isRepeatable;
        }

        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}