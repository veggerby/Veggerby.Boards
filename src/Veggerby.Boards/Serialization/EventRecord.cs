using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Recorded event with metadata for replay.
/// </summary>
public sealed record EventRecord
{
    /// <summary>
    /// Gets the event index in the sequence (0-based).
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets the event type name (e.g., "MovePieceGameEvent").
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the event data as property name to value mappings.
    /// </summary>
    public IReadOnlyDictionary<string, object> Data { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the optional timestamp when this event was created.
    /// </summary>
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// Gets the 64-bit state hash after applying this event (for verification).
    /// </summary>
    public string ResultHash { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional 128-bit state hash after applying this event.
    /// </summary>
    public string? ResultHash128 { get; init; }
}
