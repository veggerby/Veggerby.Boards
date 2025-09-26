using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Events;

/// <summary>
/// Event indicating the active player commits the current turn's primary action(s), transitioning
/// directly from the Main segment to the End segment without advancing the turn number.
/// </summary>
/// <remarks>
/// Inert when turn sequencing is disabled or when the current segment is not Main. Future extended
/// segment profiles (e.g., Commit, Resolution) may broaden applicability; current minimal profile
/// treats commit as an explicit Main→End shortcut.
/// </remarks>
public sealed class TurnCommitEvent : IGameEvent, IPhaseControlGameEvent
{
}