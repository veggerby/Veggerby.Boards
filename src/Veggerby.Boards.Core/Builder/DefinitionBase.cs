using System;

namespace Veggerby.Boards.Core.Builder;

public abstract class DefinitionBase
{
    protected GameBuilder Builder { get; }

    public DefinitionBase(GameBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        Builder = builder;
    }
}