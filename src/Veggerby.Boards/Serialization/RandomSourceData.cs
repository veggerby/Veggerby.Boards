namespace Veggerby.Boards.Serialization;

/// <summary>
/// Serialized random source state for deterministic replay.
/// </summary>
public sealed record RandomSourceData
{
    /// <summary>
    /// Gets the random source seed.
    /// </summary>
    public ulong Seed { get; init; }

    /// <summary>
    /// Gets the random source type name (e.g., "XorShiftRandomSource").
    /// </summary>
    public string TypeName { get; init; } = string.Empty;
}
