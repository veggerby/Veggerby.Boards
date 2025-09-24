using Veggerby.Boards.Internal.Acceleration;
using Veggerby.Boards.Internal.Paths;
using Veggerby.Boards.Internal.Topology;

namespace Veggerby.Boards;

/// <summary>
/// Aggregates optional internal acceleration capabilities (board shape, compiled patterns, occupancy indexes, path resolvers, bitboards).
/// </summary>
/// <remarks>
/// Replaces the prior generic EngineServices dictionary to make capability wiring explicit and type-safe while
/// preserving feature flag isolation. All members are internal and may be null when the corresponding feature is disabled.
/// </remarks>
internal sealed record EngineCapabilities(IBoardTopology Topology, IPathResolver PathResolver, IAccelerationContext Accel);