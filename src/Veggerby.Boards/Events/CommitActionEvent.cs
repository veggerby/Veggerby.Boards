using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Events;

/// <summary>
/// Event indicating a player commits an action during a simultaneous commitment phase.
/// </summary>
/// <remarks>
/// The committed action is staged but not applied until all required players have committed
/// and a <see cref="RevealCommitmentsEvent"/> is processed. This enables simultaneous action
/// games where players make hidden choices that are revealed together (e.g., Rock-Paper-Scissors,
/// sealed bids, Diplomacy orders).
/// </remarks>
/// <param name="Player">The player making the commitment.</param>
/// <param name="Action">The event being committed (will be applied during reveal).</param>
public sealed record CommitActionEvent(Player Player, IGameEvent Action) : IGameEvent
{
}
