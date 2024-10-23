namespace Veggerby.Boards.Core.Artifacts.Patterns;

public interface IPatternVisitor
{
    void Visit(DirectionPattern pattern);
    void Visit(FixedPattern pattern);
    void Visit(MultiDirectionPattern pattern);
    void Visit(NullPattern pattern);
    void Visit(AnyPattern pattern);
}