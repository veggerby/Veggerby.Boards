using System.Collections.Generic;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Snapshot of game state with integrity hash and serialized artifact states.
/// </summary>
public sealed record GameStateSnapshot
{
    /// <summary>
    /// Gets the 64-bit state hash (FNV-1a) for integrity verification.
    /// </summary>
    public string Hash { get; init; } = string.Empty;

    /// <summary>
    /// Gets the 128-bit state hash (xxHash) for enhanced integrity verification.
    /// </summary>
    public string? Hash128 { get; init; }

    /// <summary>
    /// Gets the serialized artifact states keyed by artifact ID.
    /// </summary>
    /// <remarks>
    /// Each entry maps an artifact identifier to its serialized state representation.
    /// The value is a dictionary of property name to property value for deterministic reconstruction.
    /// </remarks>
    public IReadOnlyDictionary<string, object> Artifacts { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the turn state information (optional; may be null for games without turn structure).
    /// </summary>
    public TurnStateData? Turn { get; init; }

    /// <summary>
    /// Gets the random source state (optional; may be null for non-random games).
    /// </summary>
    public RandomSourceData? Random { get; init; }
}
