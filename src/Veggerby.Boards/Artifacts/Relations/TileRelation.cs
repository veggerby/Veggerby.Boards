using System;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Represents a directional relation from one tile to another with an associated distance.
/// </summary>
public class TileRelation : ArtifactRelation<Tile, Tile>
{
    /// <summary>
    /// Gets the direction of travel from <see cref="ArtifactRelation{TFrom, TTo}.From"/> to <see cref="ArtifactRelation{TFrom, TTo}.To"/>.
    /// </summary>
    public Direction Direction { get; }

    /// <summary>
    /// Gets the distance in steps along this relation (must be positive).
    /// </summary>
    public int Distance { get; }

    /// <summary>
    /// Initializes a new tile relation.
    /// </summary>
    /// <param name="from">Origin tile.</param>
    /// <param name="to">Destination tile.</param>
    /// <param name="direction">Direction descriptor.</param>
    /// <param name="distance">Positive distance value (default 1).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="distance"/> is not positive.</exception>
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

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{GetType().Name} {From} {Distance}x{Direction} {To}";
    }
}