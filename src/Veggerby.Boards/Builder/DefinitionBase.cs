using System;

namespace Veggerby.Boards.Builder;

/// <summary>
/// Base type for fluent builder definition nodes. Provides access to the owning <see cref="GameBuilder"/>.
/// </summary>
public abstract class DefinitionBase
{
    /// <summary>
    /// Gets the associated <see cref="GameBuilder"/> used to register new definitions.
    /// </summary>
    protected GameBuilder Builder
    {
        get;
    }

    /// <summary>
    /// Initializes the definition with the owning <see cref="GameBuilder"/>.
    /// </summary>
    /// <param name="builder">The game builder orchestrating the fluent configuration.</param>
    public DefinitionBase(GameBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        Builder = builder;
    }
}