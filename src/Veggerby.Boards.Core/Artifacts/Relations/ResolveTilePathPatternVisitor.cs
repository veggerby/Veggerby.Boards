using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Algorithm.Graphs;
using Veggerby.Boards.Core.Artifacts.Patterns;

namespace Veggerby.Boards.Core.Artifacts.Relations;

public class ResolveTilePathPatternVisitor : IPatternVisitor
{
    public Board Board { get; }
    public Tile From { get; }
    public Tile To { get; }
    public TilePath ResultPath { get; private set; }

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

    public void Visit(NullPattern pattern)
    {
        ResultPath = null;
    }

    public void Visit(FixedPattern pattern)
    {
        var from = From;
        TilePath path = null;
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
        ResultPath = To.Equals(path.To) ? path : null;
    }

    public void Visit(DirectionPattern pattern)
    {
        ResultPath = GetPathFromDirection(pattern.Direction, pattern.IsRepeatable);
    }

    public void Visit(AnyPattern pattern)
    {
        // https://en.wikipedia.org/wiki/Johnson%27s_algorithm
        var q = new Tile("q");
        var graph = new Graph<Tile>(Board.Tiles, Board.TileRelations.Select(x => new Edge<Tile>(x.From, x.To, x.Distance)));

        var algorithm = new JohnsonsAlgorithm();
        var paths = algorithm.GetShortestPath(graph, q);

        var path = paths.SingleOrDefault(x => x.From.Equals(From) && x.To.Equals(To));

        if (path is null)
        {
            ResultPath = null;
            return;
        }

        TilePath resultPath = null;

        foreach (var edge in path.Edges)
        {
            resultPath = TilePath.Create(resultPath, Board.GetTileRelation(edge.From, edge.To));
        }

        ResultPath = resultPath;
    }

    private TilePath GetPathFromDirection(Direction direction, bool isRepeatable)
    {
        var from = From;
        TilePath path = null;
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