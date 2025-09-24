using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Compiled;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.Internal.Occupancy;
using Veggerby.Boards.Internal.Paths;

namespace Veggerby.Boards;

/// <summary>
/// Aggregates optional internal acceleration capabilities (board shape, compiled patterns, occupancy indexes, path resolvers, bitboards).
/// </summary>
/// <remarks>
/// Replaces the prior generic EngineServices dictionary to make capability wiring explicit and type-safe while
/// preserving feature flag isolation. All members are internal and may be null when the corresponding feature is disabled.
/// </remarks>
internal sealed class EngineCapabilities
{
    internal BoardShape Shape { get; set; }
    internal PieceMapServices PieceMap { get; set; }
    internal BitboardServices Bitboards { get; set; }
    internal CompiledPatternServices CompiledPatterns { get; set; }
    internal AttackGeneratorServices Attacks { get; set; }
    internal IOccupancyIndex Occupancy { get; set; }
    internal IPathResolver PathResolver { get; set; } // decorated post construction
}