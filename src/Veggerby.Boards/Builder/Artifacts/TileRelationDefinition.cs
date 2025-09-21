using System;

namespace Veggerby.Boards.Builder.Artifacts;

/// <summary>
/// Fluent definition describing a directional relationship between two tiles, optionally with a distance.
/// </summary>
/// <remarks>
/// Relations are declared in the context of a parent <see cref="TileDefinition"/> to allow chaining back using <see cref="Done"/>.
/// </remarks>
public class TileRelationDefinition(GameBuilder builder, TileDefinition tileDefintion) : DefinitionBase(builder)
{
    private readonly TileDefinition _tileDefintion = tileDefintion;

    /// <summary>
    /// Gets the origin tile identifier.
    /// </summary>
    public string FromTileId { get; private set; }
    /// <summary>
    /// Gets the destination tile identifier.
    /// </summary>
    public string ToTileId { get; private set; }
    /// <summary>
    /// Gets the directional identifier (e.g., N, NE) if supplied.
    /// </summary>
    public string DirectionId { get; private set; }
    /// <summary>
    /// Gets the distance associated with the relation. Defaults to 0 when not set.
    /// </summary>
    public int Distance { get; private set; }

    /// <summary>
    /// Sets the origin tile identifier.
    /// </summary>
    /// <param name="from">Source tile id.</param>
    /// <returns>The same relation definition for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="from"/> is null or empty.</exception>
    public TileRelationDefinition FromTile(string from)
    {
        if (string.IsNullOrEmpty(from))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(from));
        }

        FromTileId = from;
        return this;
    }

    /// <summary>
    /// Sets the destination tile identifier.
    /// </summary>
    /// <param name="to">Destination tile id.</param>
    /// <returns>The same relation definition for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="to"/> is null or empty.</exception>
    public TileRelationDefinition ToTile(string to)
    {
        if (string.IsNullOrEmpty(to))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(to));
        }

        ToTileId = to;
        return this;
    }

    /// <summary>
    /// Specifies a directional label for the relation.
    /// </summary>
    /// <param name="direction">Directional identifier.</param>
    /// <returns>The same relation definition for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="direction"/> is null or empty.</exception>
    public TileRelationDefinition InDirection(string direction)
    {
        if (string.IsNullOrEmpty(direction))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(direction));
        }

        DirectionId = direction;
        return this;
    }

    /// <summary>
    /// Sets the distance value for the relation.
    /// </summary>
    /// <param name="distance">Positive integer distance (semantic meaning is game-specific).</param>
    /// <returns>The same relation definition for chaining.</returns>
    public TileRelationDefinition WithDistance(int distance)
    {
        Distance = distance;
        return this;
    }

    /// <summary>
    /// Returns to the parent tile definition.
    /// </summary>
    public TileDefinition Done()
    {
        return _tileDefintion;
    }
}