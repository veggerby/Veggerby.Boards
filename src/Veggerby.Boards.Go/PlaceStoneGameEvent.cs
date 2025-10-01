using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Go;

/// <summary>
/// Represents intent to place a stone owned by the active player onto an empty intersection.
/// </summary>
/// <remarks>
/// Creates a new <see cref="PlaceStoneGameEvent"/>.
/// </remarks>
/// <param name="stone">Stone piece artifact being placed.</param>
/// <param name="target">Destination tile (must be empty).</param>
public sealed class PlaceStoneGameEvent(Piece stone, Tile target) : IGameEvent
{
    /// <summary>Gets the stone to place.</summary>
    public Piece Stone { get; } = stone ?? throw new System.ArgumentNullException(nameof(stone));
    /// <summary>Gets the target intersection tile.</summary>
    public Tile Target { get; } = target ?? throw new System.ArgumentNullException(nameof(target));
}