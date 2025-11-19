using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Chess;

namespace Veggerby.Boards.Tests.Chess.Support;

/// <summary>
/// Helper to assert that all pieces added in a scenario builder have ChessPieceMetadata attached.
/// Fail-fast to surface drift when adding new custom piece ids without metadata.
/// </summary>
internal static class MetadataCoverageGuard
{
    public static void AssertAllPiecesCovered(GameBuilder builder)
    {
        var compiled = builder.Compile();
        var piecesWithoutMetadata = compiled.Game.Artifacts
            .OfType<Piece>()
            .Where(p => p.Metadata is not ChessPieceMetadata)
            .Select(p => p.Id)
            .ToArray();

        if (piecesWithoutMetadata.Any())
        {
            var message = $"Pieces missing ChessPieceMetadata: [{string.Join(", ", piecesWithoutMetadata)}]";
            throw new System.InvalidOperationException(message);
        }
    }
}