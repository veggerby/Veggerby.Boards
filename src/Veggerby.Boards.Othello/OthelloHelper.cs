using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Helper methods for working with Othello disc states.
/// </summary>
public static class OthelloHelper
{
    /// <summary>
    /// Gets the current color of a disc, accounting for flips.
    /// </summary>
    /// <param name="piece">The disc piece.</param>
    /// <param name="state">The current game state.</param>
    /// <returns>The current color of the disc.</returns>
    public static OthelloDiscColor GetCurrentDiscColor(Piece piece, GameState state)
    {
        System.ArgumentNullException.ThrowIfNull(piece);
        System.ArgumentNullException.ThrowIfNull(state);

        var metadata = piece.Metadata as OthelloDiscMetadata;
        if (metadata == null)
        {
            throw new System.InvalidOperationException("Piece does not have Othello metadata");
        }

        var originalColor = metadata.Color;

        // Count the number of flips
        var flipStates = state.GetStates<FlippedDiscState>()
            .Where(fs => fs.Artifact.Equals(piece))
            .ToList();

        // Odd number of flips = opposite color, even = original color
        if (flipStates.Count % 2 == 1)
        {
            return originalColor == OthelloDiscColor.Black ? OthelloDiscColor.White : OthelloDiscColor.Black;
        }

        return originalColor;
    }
}
