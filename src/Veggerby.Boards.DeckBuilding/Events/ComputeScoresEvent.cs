using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Event requesting computation of per-player victory point totals. Idempotent: ignored if already computed.
/// </summary>
public sealed class ComputeScoresEvent : IGameEvent
{
}