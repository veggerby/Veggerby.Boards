using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Events;

/// <summary>
/// Event triggering the reveal and simultaneous resolution of all committed player actions.
/// </summary>
/// <remarks>
/// This event should be processed only after all required players have committed their actions
/// during a commitment phase. It causes all staged events to be applied deterministically in
/// a game-defined order (typically player ID order for tie-breaking). After reveal, the staged
/// events are cleared and the commitment phase ends.
/// </remarks>
public sealed record RevealCommitmentsEvent : IGameEvent
{
}
