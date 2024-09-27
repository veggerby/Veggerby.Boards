namespace Veggerby.Boards.Core.Artifacts.Patterns;

public interface IPattern
{
    void Accept(IPatternVisitor visitor);
}