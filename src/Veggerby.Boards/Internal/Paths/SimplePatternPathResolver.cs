using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Paths;

/// <summary>
/// Fallback path resolver that iterates a piece's patterns using the existing <see cref="ResolveTilePathPatternVisitor"/> logic.
/// </summary>
internal sealed class SimplePatternPathResolver(Board board) : IPathResolver
{
    private readonly Board _board = board;

    public TilePath Resolve(Piece piece, Tile from, Tile to, GameState state)
    {
        if (piece is null || from is null || to is null || from == to)
        {
            return null;
        }

        return piece.Patterns
            .Select(pattern =>
            {
                var visitor = new ResolveTilePathPatternVisitor(_board, from, to);
                pattern.Accept(visitor);
                return visitor.ResultPath;
            })
            .Where(p => p is not null)
            .OrderBy(p => p.Distance)
            .FirstOrDefault();
    }
}