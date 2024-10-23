using System;

namespace Veggerby.Boards.Core.Builder.Artifacts;

public class TileRelationDefinition(GameBuilder builder, TileDefinition tileDefintion) : DefinitionBase(builder)
{
    private readonly TileDefinition _tileDefintion = tileDefintion;

    public string FromTileId { get; private set; }
    public string ToTileId { get; private set; }
    public string DirectionId { get; private set; }
    public int Distance { get; private set; }

    public TileRelationDefinition FromTile(string from)
    {
        if (string.IsNullOrEmpty(from))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(from));
        }

        FromTileId = from;
        return this;
    }

    public TileRelationDefinition ToTile(string to)
    {
        if (string.IsNullOrEmpty(to))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(to));
        }

        ToTileId = to;
        return this;
    }

    public TileRelationDefinition InDirection(string direction)
    {
        if (string.IsNullOrEmpty(direction))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(direction));
        }

        DirectionId = direction;
        return this;
    }

    public TileRelationDefinition WithDistance(int distance)
    {
        Distance = distance;
        return this;
    }

    public TileDefinition Done()
    {
        return _tileDefintion;
    }
}