using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Tests.TestHelpers;

/// <summary>
/// Helper utilities for resolving concrete <see cref="TilePath"/> instances in tests without duplicating pattern traversal logic.
/// Mirrors the logic in <c>GameExtensions.Move</c> but returns the path instead of applying the move.
/// </summary>
internal static class TestPathHelper
{
    /// <summary>
    /// Attempts to resolve the first valid shortest path from the specified origin piece to one of the candidate target tile ids.
    /// Targets are evaluated in provided order; within each target the piece's patterns are evaluated in declaration order.
    /// </summary>
    /// <param name="game">Game context.</param>
    /// <param name="piece">The piece to move.</param>
    /// <param name="fromTileId">The starting tile id (current tile sanity check).</param>
    /// <param name="targetTileIds">Ordered candidate destination tile ids (first resolvable wins).</param>
    /// <returns>The first valid <see cref="TilePath"/> or <c>null</c> if none resolve.</returns>
    public static TilePath? ResolveFirstValidPath(Game game, Piece piece, string fromTileId, params string[] targetTileIds)
    {
        var fromTile = game.GetTile(fromTileId);
        if (fromTile is null)
        {
            return null;
        }
        // We cannot access a GameProgress here (not passed); ensure the piece logically starts on fromTile by checking patterns from 'fromTile'.

        foreach (var targetId in targetTileIds)
        {
            var toTile = game.GetTile(targetId);
            if (toTile is null)
            {
                continue;
            }
            foreach (var pattern in piece.Patterns)
            {
                var visitor = new ResolveTilePathPatternVisitor(game.Board, fromTile, toTile);
                pattern.Accept(visitor);
                if (visitor.ResultPath is not null)
                {
                    return visitor.ResultPath;
                }
            }
        }

        return null;
    }
}