using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Events;

/// <summary>
/// Event indicating the active player elects to pass the remainder of their turn, forcing immediate
/// advancement to the next turn (TurnNumber + 1, Segment reset to Start) with active player rotation.
/// </summary>
/// <remarks>
/// Turn passing is only honored when <c>FeatureFlags.EnableTurnSequencing</c> is enabled. It provides
/// an explicit alternative to completing all segments (e.g., Start→Main→End) when no further actions
/// are desired or legal. Future segment models (Upkeep, Roll, Commit) may restrict pass eligibility
/// to certain segments; current minimal profile allows passing from any segment.
/// </remarks>
public sealed class TurnPassEvent : IGameEvent, IPhaseControlGameEvent
{
}