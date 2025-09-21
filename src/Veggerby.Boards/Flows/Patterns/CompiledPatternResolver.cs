using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Flows.Patterns;

/// <summary>
/// Resolves a path using compiled patterns for a piece if available; returns null when no pattern matches.
/// </summary>
internal sealed class CompiledPatternResolver
{
    private readonly CompiledPatternTable _table;
    private readonly Board _board;

    public CompiledPatternResolver(CompiledPatternTable table, Board board)
    {
        _table = table;
        _board = board;
    }

    public TilePath Resolve(Piece piece, Tile from, Tile to)
    {
        if (!_table.ByPiece.TryGetValue(piece.Id, out var piecePatterns) || piecePatterns.Patterns.Count == 0)
        {
            return null; // fallback to legacy expected
        }

        TilePath best = null;
        foreach (var pattern in piecePatterns.Patterns)
        {
            switch (pattern.Kind)
            {
                case CompiledPatternKind.Fixed:
                    {
                        var current = from;
                        TilePath path = null;
                        var ok = true;
                        foreach (var dir in pattern.Directions)
                        {
                            var rel = _board.GetTileRelation(current, dir);
                            if (rel is null)
                            {
                                ok = false; break;
                            }
                            path = TilePath.Create(path, rel);
                            current = rel.To;
                        }
                        if (ok && path is not null && path.To.Equals(to))
                        {
                            best = SelectShortest(best, path);
                        }
                    }
                    break;
                case CompiledPatternKind.Ray:
                    {
                        var dir = pattern.Directions[0];
                        var current = from;
                        TilePath path = null;
                        while (true)
                        {
                            var rel = _board.GetTileRelation(current, dir);
                            if (rel is null) break;
                            path = TilePath.Create(path, rel);
                            current = rel.To;
                            if (current.Equals(to))
                            {
                                best = SelectShortest(best, path);
                                break;
                            }
                            if (!pattern.IsRepeatable) break;
                        }
                    }
                    break;
                case CompiledPatternKind.MultiRay:
                    foreach (var dir in pattern.Directions)
                    {
                        var current = from;
                        TilePath path = null;
                        while (true)
                        {
                            var rel = _board.GetTileRelation(current, dir);
                            if (rel is null) break;
                            path = TilePath.Create(path, rel);
                            current = rel.To;
                            if (current.Equals(to))
                            {
                                best = SelectShortest(best, path);
                                break; // found path for this direction
                            }
                            if (!pattern.IsRepeatable) break;
                        }
                    }
                    break;
            }
        }
        return best;
    }

    private static TilePath SelectShortest(TilePath currentBest, TilePath candidate)
    {
        if (candidate is null) return currentBest;
        if (currentBest is null) return candidate;
        return candidate.Distance < currentBest.Distance ? candidate : currentBest;
    }
}