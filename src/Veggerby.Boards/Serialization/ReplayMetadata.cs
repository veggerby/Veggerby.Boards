using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Metadata describing a replay including game type, players, and context.
/// </summary>
public sealed record ReplayMetadata
{
    /// <summary>
    /// Gets the game type identifier (e.g., "chess", "go", "backgammon").
    /// </summary>
    public string GameType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the list of player identifiers participating in the game.
    /// </summary>
    public IReadOnlyList<string> Players { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the timestamp when this replay was created.
    /// </summary>
    public DateTime Created { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets an optional title for this game (e.g., "The Immortal Game").
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets optional tags categorizing this replay (e.g., "famous", "sacrifice", "endgame").
    /// </summary>
    public IReadOnlyList<string>? Tags { get; init; }

    /// <summary>
    /// Gets optional custom metadata for game-specific or application-specific data.
    /// </summary>
    public IReadOnlyDictionary<string, string>? CustomMetadata { get; init; }
}
