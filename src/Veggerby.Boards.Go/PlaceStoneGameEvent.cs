using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Go;

/// <summary>
/// Represents intent to place a stone owned by the active player onto an empty intersection.
/// </summary>
public sealed class PlaceStoneGameEvent : IGameEvent
{
    /// <summary>Gets the stone to place.</summary>
    public Piece Stone { get; }
    /// <summary>Gets the target intersection tile.</summary>
    public Tile Target { get; }

    /// <summary>
    /// Creates a new <see cref="PlaceStoneGameEvent"/>.
    /// </summary>
    /// <param name="stone">Stone piece artifact being placed.</param>
    /// <param name="target">Destination tile (must be empty).</param>
    public PlaceStoneGameEvent(Piece stone, Tile target)
    {
        Stone = stone ?? throw new System.ArgumentNullException(nameof(stone));
        Target = target ?? throw new System.ArgumentNullException(nameof(target));
    }
}