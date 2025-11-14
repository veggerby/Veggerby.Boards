using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Compiled;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Flows.Patterns;

/// <summary>
/// Resolves a path using compiled patterns for a piece if available; returns null when no pattern matches.
/// </summary>
internal sealed class CompiledPatternResolver(CompiledPatternTable table, Board board, BoardAdjacencyCache? adjacency, BoardShape shape) : ICompiledPatternResolver
{
    private readonly CompiledPatternTable _table = table;
    private readonly Board _board = board;
    private readonly BoardAdjacencyCache? _adjacency = adjacency;
    private readonly BoardShape _shape = shape;

    public bool TryResolve(Piece piece, Tile from, Tile to, out TilePath? path)
    {
        path = null;
        if (!_table.ByPiece.TryGetValue(piece.Id, out var piecePatterns) || piecePatterns.Patterns.Count == 0)
        {
            return false; // fallback to legacy expected
        }

        TilePath? best = null;
        foreach (var pattern in piecePatterns.Patterns)
        {
            switch (pattern.Kind)
            {
                case CompiledPatternKind.Fixed:
                    {
                        var current = from;
                        TilePath? localPath = null;
                        var ok = true;
                        foreach (var dir in pattern.Directions)
                        {
                            var rel = Resolve(current, dir);
                            if (rel is null)
                            {
                                ok = false;
                                break;
                            }
                            localPath = TilePath.Create(localPath, rel);
                            current = rel.To;
                        }
                        if (ok && localPath is not null && localPath.To.Equals(to))
                        {
                            best = SelectShortest(best, localPath);
                        }
                    }
                    break;
                case CompiledPatternKind.Ray:
                    {
                        var dir = pattern.Directions[0];
                        var current = from;
                        TilePath? localPath = null;
                        while (true)
                        {
                            var rel = Resolve(current, dir);
                            if (rel is null)
                                break;
                            localPath = TilePath.Create(localPath, rel);
                            current = rel.To;
                            if (current.Equals(to))
                            {
                                best = SelectShortest(best, localPath);
                                break;
                            }
                            if (!pattern.IsRepeatable)
                                break;
                        }
                    }
                    break;
                case CompiledPatternKind.MultiRay:
                    foreach (var dir in pattern.Directions)
                    {
                        var current = from;
                        TilePath? localPath = null;
                        while (true)
                        {
                            var rel = Resolve(current, dir);
                            if (rel is null)
                                break;
                            localPath = TilePath.Create(localPath, rel);
                            current = rel.To;
                            if (current.Equals(to))
                            {
                                best = SelectShortest(best, localPath);
                                break; // found path for this direction
                            }
                            if (!pattern.IsRepeatable)
                                break;
                        }
                    }
                    break;
            }
        }
        path = best;
        return path is not null;
    }

    private static TilePath? SelectShortest(TilePath? currentBest, TilePath? candidate)
    {
        if (candidate is null)
            return currentBest;
        if (currentBest is null)
            return candidate;
        return candidate.Distance < currentBest.Distance ? candidate : currentBest;
    }

    private TileRelation? Resolve(Tile from, Direction dir)
    {
        // Fast path: BoardShape neighbor lookup (always enabled - graduated feature)
        if (_shape is not null && _shape.TryGetNeighbor(from, dir, out var neighbor))
        {
            // BoardShape confirmed existence; retrieve relation from board
            return _board.GetTileRelation(from, dir);
        }

        // Fallback: board relation scan
        return _board.GetTileRelation(from, dir);
    }
}