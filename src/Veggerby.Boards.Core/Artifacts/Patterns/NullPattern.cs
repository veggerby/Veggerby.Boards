namespace Veggerby.Boards.Core.Artifacts.Patterns
{
    public class NullPattern : IPattern 
    {
        public void Accept(IPatternVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}