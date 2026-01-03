using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Represents intent to place a disc owned by the active player onto an empty square.
/// </summary>
/// <remarks>
/// Creates a new <see cref="PlaceDiscGameEvent"/>.
/// </remarks>
/// <param name="disc">Disc piece artifact being placed.</param>
/// <param name="target">Destination tile (must be empty and must flip at least one opponent disc).</param>
public sealed class PlaceDiscGameEvent(Piece disc, Tile target) : IGameEvent
{
    /// <summary>Gets the disc to place.</summary>
    public Piece Disc { get; } = disc ?? throw new System.ArgumentNullException(nameof(disc));

    /// <summary>Gets the target tile.</summary>
    public Tile Target { get; } = target ?? throw new System.ArgumentNullException(nameof(target));
}
