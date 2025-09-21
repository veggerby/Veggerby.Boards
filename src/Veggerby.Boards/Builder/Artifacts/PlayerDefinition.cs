using System;

namespace Veggerby.Boards.Builder.Artifacts;

/// <summary>
/// Fluent definition for configuring a player artifact in the game specification.
/// </summary>
public class PlayerDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    /// <summary>
    /// Gets the configured player identifier.
    /// </summary>
    public string PlayerId { get; private set; }

    /// <summary>
    /// Assigns the identifier for the player being defined.
    /// </summary>
    /// <param name="id">Unique player identifier.</param>
    /// <returns>The same definition instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public PlayerDefinition WithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        PlayerId = id;
        return this;
    }
}