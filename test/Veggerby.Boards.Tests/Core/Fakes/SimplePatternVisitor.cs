using System;

using Veggerby.Boards.Artifacts.Patterns;

namespace Veggerby.Boards.Tests.Core.Fakes;

public class SimplePatternVisitor : IPatternVisitor
{
    public Type Type { get; private set; } = typeof(NullPattern); // initialized to satisfy non-null contract

    public void Visit(MultiDirectionPattern pattern)
    {
        Type = typeof(MultiDirectionPattern);
    }

    public void Visit(AnyPattern pattern)
    {
        Type = typeof(AnyPattern);
    }

    public void Visit(NullPattern pattern)
    {
        Type = typeof(NullPattern);
    }

    public void Visit(FixedPattern pattern)
    {
        Type = typeof(FixedPattern);
    }

    public void Visit(DirectionPattern pattern)
    {
        Type = typeof(DirectionPattern);
    }
}