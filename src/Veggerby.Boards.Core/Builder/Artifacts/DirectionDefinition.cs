using System;

namespace Veggerby.Boards.Core.Builder.Artifacts;

/// <summary>
/// Fluent definition for a board direction artifact.
/// </summary>
public class DirectionDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    /// <summary>
    /// Gets the configured direction identifier.
    /// </summary>
    public string DirectionId { get; private set; }

    /// <summary>
    /// Sets the direction identifier.
    /// </summary>
    /// <param name="id">Unique direction id.</param>
    public DirectionDefinition WithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        DirectionId = id;
        return this;
    }
}