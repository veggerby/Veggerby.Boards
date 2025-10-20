using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Algorithm.Graphs;
using Veggerby.Boards.Artifacts.Patterns;

namespace Veggerby.Boards.Artifacts.Relations;

/// <summary>
/// Visitor that resolves a movement pattern (<see cref="IPattern"/>) into a concrete <see cref="TilePath"/> between two tiles.
/// </summary>
public class ResolveTilePathPatternVisitor : IPatternVisitor
{
    /// <summary>
    /// Gets the board context for relation lookup.
    /// </summary>
    public Board Board
    {
        get;
    }
    /// <summary>
    /// Gets the origin tile.
    /// </summary>
    public Tile From
    {
        get;
    }
    /// <summary>
    /// Gets the destination tile.
    /// </summary>
    public Tile To
    {
        get;
    }
    /// <summary>
    /// Gets the resolved path, if any.
    /// </summary>
    public TilePath? ResultPath
    {
        get; private set;
    }

    /// <summary>
    /// Initializes the visitor with board and endpoints.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when origin equals destination.</exception>
    public ResolveTilePathPatternVisitor(Board board, Tile from, Tile to)
    {
        ArgumentNullException.ThrowIfNull(board);

        ArgumentNullException.ThrowIfNull(from);

        ArgumentNullException.ThrowIfNull(to);

        if (from.Equals(to))
        {
            throw new ArgumentException("To cannot be the same af From", nameof(to));
        }

        Board = board;
        From = from;
        To = to;
    }

    /// <summary>
    /// Selects the shortest valid path among multiple directions, optionally repeating steps.
    /// </summary>
    public void Visit(MultiDirectionPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
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

    /// <summary>
    /// Handles a null pattern by clearing the result path.
    /// </summary>
    public void Visit(NullPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ResultPath = null;
    }

    /// <summary>
    /// Attempts to resolve a fixed ordered set of directions into a concrete path.
    /// </summary>
    public void Visit(FixedPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
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
        if (path is not null && To.Equals(path.To))
        {
            ResultPath = path;
        }
        else
        {
            ResultPath = null;
        }
    }

    /// <summary>
    /// Resolves a single direction pattern (repeatable or single step) into a path.
    /// </summary>
    public void Visit(DirectionPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ResultPath = GetPathFromDirection(pattern.Direction, pattern.IsRepeatable);
    }

    /// <summary>
    /// Uses Johnson's algorithm to find the shortest path between the endpoints regardless of direction constraints.
    /// </summary>
    public void Visit(AnyPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        // https://en.wikipedia.org/wiki/Johnson%27s_algorithm
        var q = new Tile("q");
        var graph = new Graph<Tile>(Board.Tiles, Board.TileRelations.Select(x => new Edge<Tile>(x.From, x.To, x.Distance)));

        var paths = JohnsonsAlgorithm.GetShortestPath(graph, q);

        var path = paths.SingleOrDefault(x => x.From.Equals(From) && x.To.Equals(To));

        if (path == default)
        {
            ResultPath = null;
            return;
        }

        TilePath? resultPath = null;

        foreach (var edge in path.Edges)
        {
            var relation = Board.GetTileRelation(edge.From, edge.To);
            if (relation is null)
            {
                ResultPath = null;
                return;
            }
            resultPath = TilePath.Create(resultPath, relation);
        }

        ResultPath = resultPath;
    }

    /// <summary>
    /// Builds a path by stepping repeatedly in a single direction until destination reached or blocked.
    /// </summary>
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
                return path;
            }

            from = relation.To;
        }

        return null;
    }
}