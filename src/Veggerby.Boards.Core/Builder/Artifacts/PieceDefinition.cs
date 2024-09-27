using System;

namespace Veggerby.Boards.Core.Builder.Artifacts;

public class PieceDefinition(GameBuilder builder) : DefinitionBase(builder)
{
    public string PieceId { get; private set; }
    public string PlayerId { get; private set; }

    public PieceDefinition WithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        PieceId = id;
        return this;
    }

    public PieceDefinition WithOwner(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(id));
        }

        PlayerId = id;
        return this;
    }

    public PieceDefinition OnTile(string tileId)
    {
        if (string.IsNullOrEmpty(tileId))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(tileId));
        }

        Builder.AddPieceOnTile(PieceId, tileId);
        return this;
    }

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

    public PieceDefinition HasPattern(params string[] ids)
    {
        var direction = new PieceDirectionPatternDefinition(Builder, this).WithDirection(ids);
        Builder.Add(direction);
        return this;
    }
}