namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class AnyPattern : IPattern 
    {
        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}