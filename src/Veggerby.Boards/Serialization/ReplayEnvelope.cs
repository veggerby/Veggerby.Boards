using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Container for serialized game state and event history suitable for persistence and deterministic replay.
/// </summary>
/// <remarks>
/// This envelope format provides a stable, versioned structure for saving/loading games,
/// reproducing exact game sequences, and sharing games across platforms.
/// </remarks>
public sealed record ReplayEnvelope
{
    /// <summary>
    /// Gets the format identifier. Always "veggerby-boards-replay".
    /// </summary>
    public string Format { get; init; } = "veggerby-boards-replay";

    /// <summary>
    /// Gets the format version (e.g., "1.0").
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Gets metadata describing the game and replay context.
    /// </summary>
    public ReplayMetadata Metadata { get; init; } = new();

    /// <summary>
    /// Gets the initial game state snapshot.
    /// </summary>
    public GameStateSnapshot InitialState { get; init; } = new();

    /// <summary>
    /// Gets the ordered sequence of events applied to the game.
    /// </summary>
    public IReadOnlyList<EventRecord> Events { get; init; } = Array.Empty<EventRecord>();

    /// <summary>
    /// Gets the final game state snapshot (optional; may be null for incomplete games).
    /// </summary>
    public GameStateSnapshot? FinalState { get; init; }
}
