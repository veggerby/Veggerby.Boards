using System;

namespace Veggerby.Boards.Builder.Artifacts;

/// <summary>
/// Fluent definition used to configure a single tile artifact and its relations to other tiles.
/// </summary>
public class TileDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    /// <summary>
    /// Gets the configured tile identifier.
    /// </summary>
    public string TileId { get; private set; } = null!; // LIFECYCLE: set by WithId() before Build()

    /// <summary>
    /// Sets the identifier for the tile.
    /// </summary>
    /// <param name="id">Unique tile identifier.</param>
    /// <returns>The same definition instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(TileId))]
    public TileDefinition WithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        TileId = id;
        return this;
    }

    /// <summary>
    /// Declares a relation from this tile to another tile.
    /// </summary>
    /// <param name="id">Identifier of the destination tile.</param>
    /// <returns>A relation definition for further configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public TileRelationDefinition WithRelationTo(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        var relation = new TileRelationDefinition(Builder, this)
            .FromTile(this.TileId)
            .ToTile(id);

        Builder.Add(relation);
        return relation;
    }

    /// <summary>
    /// Declares a relation from another tile to this tile.
    /// </summary>
    /// <param name="id">Identifier of the source tile.</param>
    /// <returns>A relation definition for further configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public TileRelationDefinition WithRelationFrom(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        var relation = new TileRelationDefinition(Builder, this)
            .FromTile(id)
            .ToTile(this.TileId);

        Builder.Add(relation);
        return relation;
    }
}