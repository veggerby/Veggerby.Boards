namespace Veggerby.Boards.Serialization;

/// <summary>
/// Serialized turn state information.
/// </summary>
public sealed record TurnStateData
{
    /// <summary>
    /// Gets the active player identifier.
    /// </summary>
    public string Player { get; init; } = string.Empty;

    /// <summary>
    /// Gets the turn number (1-based).
    /// </summary>
    public int Number { get; init; }

    /// <summary>
    /// Gets the turn segment name (optional; used for multi-segment turns).
    /// </summary>
    public string? Segment { get; init; }
}
