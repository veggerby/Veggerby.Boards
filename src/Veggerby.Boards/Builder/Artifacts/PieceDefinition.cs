using System;

namespace Veggerby.Boards.Builder.Artifacts;

/// <summary>
/// Fluent definition used to configure a piece artifact, its owner, initial tile and movement patterns.
/// </summary>
public class PieceDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    /// <summary>
    /// Gets the configured piece identifier.
    /// </summary>
    public string PieceId { get; private set; } = null!; // LIFECYCLE: set by WithId() before Build()
    /// <summary>
    /// Gets the owner player identifier if configured.
    /// </summary>
    public string PlayerId { get; private set; } = null!; // LIFECYCLE: set by WithOwner() before Build()

    /// <summary>
    /// Sets the piece identifier.
    /// </summary>
    public PieceDefinition WithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        PieceId = id;
        return this;
    }

    /// <summary>
    /// Assigns an owner player identifier to the piece.
    /// </summary>
    public PieceDefinition WithOwner(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        PlayerId = id;
        return this;
    }

    /// <summary>
    /// Places the piece on an initial tile.
    /// </summary>
    public PieceDefinition OnTile(string tileId)
    {
        if (string.IsNullOrEmpty(tileId))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(tileId));
        }

        Builder.AddPieceOnTile(PieceId, tileId);
        return this;
    }

    /// <summary>
    /// Defines a directional movement pattern consisting of a single direction.
    /// </summary>
    public PieceDirectionPatternDefinition HasDirection(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        var direction = new PieceDirectionPatternDefinition(Builder, this).WithDirection(id);
        Builder.Add(direction);
        return direction;
    }

    /// <summary>
    /// Defines a movement pattern consisting of multiple directions.
    /// </summary>
    public PieceDefinition HasPattern(params string[] ids)
    {
        var direction = new PieceDirectionPatternDefinition(Builder, this).WithDirection(ids);
        Builder.Add(direction);
        return this;
    }
}