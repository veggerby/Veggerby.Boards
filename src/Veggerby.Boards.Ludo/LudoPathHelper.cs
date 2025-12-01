using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Ludo;

/// <summary>
/// Helper class for resolving movement paths in Ludo.
/// </summary>
/// <remarks>
/// Ludo has special path traversal rules:
/// <list type="bullet">
/// <item>Forward movement along the main track</item>
/// <item>Home entry at the player's designated entry point</item>
/// <item>Movement along the home stretch toward the finish</item>
/// </list>
/// </remarks>
public static class LudoPathHelper
{
    /// <summary>
    /// The direction ID for normal forward movement.
    /// </summary>
    public const string ForwardDirection = "forward";

    /// <summary>
    /// The direction ID for entering the home stretch.
    /// </summary>
    public const string HomeEntryDirection = "home-entry";

    /// <summary>
    /// Resolves the destination tile for a piece moving the specified number of steps.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="startTile">The starting tile.</param>
    /// <param name="steps">The number of steps to move.</param>
    /// <param name="playerColor">The player's color (for home entry detection).</param>
    /// <param name="canEnterHome">Whether the piece can enter the home stretch (has completed a lap).</param>
    /// <returns>The destination tile, or null if the path is invalid.</returns>
    public static Tile? ResolveDestination(Board board, Tile startTile, int steps, string playerColor, bool canEnterHome = false)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(startTile);
        ArgumentException.ThrowIfNullOrEmpty(playerColor);

        Tile? currentTile = startTile;

        for (int step = 0; step < steps && currentTile is not null; step++)
        {
            currentTile = GetNextTile(board, currentTile, playerColor, canEnterHome);
        }

        return currentTile;
    }

    /// <summary>
    /// Gets the complete path from a starting tile to a destination.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="startTile">The starting tile.</param>
    /// <param name="steps">The number of steps to move.</param>
    /// <param name="playerColor">The player's color (for home entry detection).</param>
    /// <param name="canEnterHome">Whether the piece can enter the home stretch.</param>
    /// <returns>The path as a list of tiles (excluding start, including destination), or null if invalid.</returns>
    public static IReadOnlyList<Tile>? ResolvePath(Board board, Tile startTile, int steps, string playerColor, bool canEnterHome = false)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(startTile);
        ArgumentException.ThrowIfNullOrEmpty(playerColor);

        var path = new List<Tile>();
        Tile? currentTile = startTile;

        for (int step = 0; step < steps && currentTile is not null; step++)
        {
            currentTile = GetNextTile(board, currentTile, playerColor, canEnterHome);

            if (currentTile is not null)
            {
                path.Add(currentTile);
            }
        }

        // Return null if we couldn't complete the full path
        if (path.Count != steps)
        {
            return null;
        }

        return path;
    }

    /// <summary>
    /// Gets the next tile in the path for a given player.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="currentTile">The current tile.</param>
    /// <param name="playerColor">The player's color.</param>
    /// <param name="canEnterHome">Whether the piece can enter the home stretch.</param>
    /// <returns>The next tile, or null if no valid path exists.</returns>
    private static Tile? GetNextTile(Board board, Tile currentTile, string playerColor, bool canEnterHome)
    {
        Tile? forwardTile = null;

        foreach (var relation in board.TileRelations)
        {
            if (!relation.From.Equals(currentTile))
            {
                continue;
            }

            // Check home entry first if allowed (takes priority for the current player)
            if (canEnterHome &&
                string.Equals(relation.Direction.Id, HomeEntryDirection, StringComparison.Ordinal) &&
                relation.To.Id.StartsWith($"home-{playerColor}-", StringComparison.Ordinal))
            {
                return relation.To;
            }

            // Track forward direction as fallback
            if (string.Equals(relation.Direction.Id, ForwardDirection, StringComparison.Ordinal))
            {
                forwardTile = relation.To;
            }
        }

        return forwardTile;
    }

    /// <summary>
    /// Checks if a piece is in its base (waiting to enter).
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <returns>True if the tile is a base tile.</returns>
    public static bool IsBaseTile(Tile tile)
    {
        ArgumentNullException.ThrowIfNull(tile);
        return tile.Id.StartsWith("base-", StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks if a tile is in the home stretch.
    /// </summary>
    /// <param name="tile">The tile to check.</param>
    /// <returns>True if the tile is a home stretch tile.</returns>
    public static bool IsHomeTile(Tile tile)
    {
        ArgumentNullException.ThrowIfNull(tile);
        return tile.Id.StartsWith("home-", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the starting track position for a player color.
    /// </summary>
    /// <param name="playerColor">The player color.</param>
    /// <returns>The starting track tile ID.</returns>
    public static string GetStartingTileId(string playerColor)
    {
        ArgumentException.ThrowIfNullOrEmpty(playerColor);

        var colorIndex = playerColor.ToLowerInvariant() switch
        {
            "red" => 0,
            "blue" => 1,
            "green" => 2,
            "yellow" => 3,
            _ => throw new ArgumentException($"Unknown player color: {playerColor}", nameof(playerColor))
        };

        return $"track-{colorIndex * 13}";
    }
}
