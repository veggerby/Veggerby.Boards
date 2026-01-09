using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Examples.DiplomacyMovement;

/// <summary>
/// Event representing a Diplomacy-style move order for a unit.
/// </summary>
/// <remarks>
/// This simplified example demonstrates simultaneous movement resolution where multiple units
/// may attempt to move to the same territory. Conflicts are resolved using player order
/// tie-breaking: the first player (by ID order) succeeds, subsequent moves to the same
/// destination fail.
/// </remarks>
/// <param name="Player">The player issuing the order.</param>
/// <param name="Unit">The unit (piece) to move.</param>
/// <param name="Destination">The tile to move to.</param>
public sealed record MoveOrderEvent(Player Player, Piece Unit, Tile Destination) : IGameEvent
{
}
