using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts.Patterns;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Resolves a tile path between two tiles constrained by an exact distance using pattern semantics.
/// </summary>
public class ResolveTilePathDistanceVisitor : IPatternVisitor
{
    /// <summary>
    /// Gets the board on which resolution occurs.
    /// </summary>
    public Board Board { get; }

    /// <summary>
    /// Gets the origin tile.
    /// </summary>
    public Tile From { get; }

    /// <summary>
    /// Gets the destination tile.
    /// </summary>
    public Tile To { get; }

    /// <summary>
    /// Gets the target distance that a resolved path must satisfy.
    /// </summary>
    public int Distance { get; }

    /// <summary>
    /// Gets a value indicating whether paths shorter than <see cref="Distance"/> may be accepted when the target tile is reached early.
    /// </summary>
    public bool AllowOvershootDistance { get; }

    /// <summary>
    /// Gets the resulting path (or null if none matched).
    /// </summary>
    public TilePath? ResultPath { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveTilePathDistanceVisitor"/> class.
    /// </summary>
    /// <param name="board">Game board instance.</param>
    /// <param name="from">Start tile.</param>
    /// <param name="to">Destination tile.</param>
    /// <param name="distance">Exact path distance to evaluate (must be positive).</param>
    /// <param name="allowOvershootDistance">If true allows reaching <paramref name="to"/> earlier than <paramref name="distance"/> then continuing until distance is met.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="distance"/> is not positive.</exception>
    public ResolveTilePathDistanceVisitor(Board board, Tile from, Tile to, int distance, bool allowOvershootDistance = false)
    {
        ArgumentNullException.ThrowIfNull(board);

        ArgumentNullException.ThrowIfNull(from);

        ArgumentNullException.ThrowIfNull(to);

        if (distance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distance));
        }

        Board = board;
        From = from;
        To = to;
        Distance = distance;
        AllowOvershootDistance = allowOvershootDistance;
    }

    /// <inheritdoc />
    public void Visit(MultiDirectionPattern pattern)
    {
    var paths = new List<TilePath>();
        foreach (var direction in pattern.Directions)
        {
            var path = GetPathFromDirection(direction, pattern.IsRepeatable);
            if (path is not null)
            {
                paths.Add(path);
            }
        }

        ResultPath = paths.Any() ? paths.OrderBy(x => x.Distance).First() : null;
    }

    /// <inheritdoc />
    public void Visit(NullPattern pattern)
    {
        ResultPath = null;
    }

    /// <inheritdoc />
    public void Visit(FixedPattern pattern)
    {
        var from = From;
    TilePath? path = null;
        foreach (var direction in pattern.Pattern)
        {
            var relation = Board.GetTileRelation(from, direction);
            if (relation is null)
            {
                ResultPath = null;
                return;
            }

            path = TilePath.Create(path, relation);
            from = relation.To;
        }

        // path will always have a value because there will always be at least ONE direction in FixedPattern
        if (path is not null && path.Distance == Distance)
        {
            ResultPath = path;
        }
        else
        {
            ResultPath = null;
        }
    }

    /// <inheritdoc />
    public void Visit(DirectionPattern pattern)
    {
        ResultPath = GetPathFromDirection(pattern.Direction, pattern.IsRepeatable);
    }

    /// <inheritdoc />
    public void Visit(AnyPattern pattern)
    {
        throw new NotImplementedException();
    }

    private TilePath? GetPathFromDirection(Direction direction, bool isRepeatable)
    {
        var from = From;
        TilePath? path = null;
        while (path is null || isRepeatable) // we have not yet taken a step or it is repeatable
        {
            var relation = Board.GetTileRelation(from, direction);

            if (relation is null)
            {
                return null;
            }

            if (relation.To.Equals(From) || (path?.Tiles.Contains(relation.To) ?? false))
            {
                // back to where we started or we crossed paths... break
                return null;
            }

            path = TilePath.Create(path, relation);

            if (To.Equals(path.To))
            {
                if (path.Distance > Distance)
                {
                    return null;
                }

                if (path.Distance < Distance && !AllowOvershootDistance)
                {
                    return null;
                }

                return path;
            }

            if (path.Distance == Distance)
            {
                return path;
            }
            else if (path.Distance > Distance)
            {
                return null;
            }

            from = relation.To;
        }

        return null;
    }
}