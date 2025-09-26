using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Inert state mutation classified event used solely for EventKind filtering benchmark heterogeneity.
/// Produces no state change; engine treats it as a state mutation kind target for skip path measurement.
/// </summary>
internal sealed class BenchmarkStateNoOpEvent : IGameEvent, IStateMutationGameEvent
{
}