using System;

namespace Veggerby.Boards.Core.Artifacts.Relations;

public class TileRelation : ArtifactRelation<Tile, Tile>
{
    public Direction Direction { get; }

    public int Distance { get; }

    public TileRelation(Tile from, Tile to, Direction direction, int distance = 1) : base(from, to)
    {
        ArgumentNullException.ThrowIfNull(direction);

        if (distance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be positive and non-zero");
        }

        Direction = direction;
        Distance = distance;
    }

    public override string ToString()
    {
        return $"{GetType().Name} {From} {Distance}x{Direction} {To}";
    }

}