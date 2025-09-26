using Veggerby.Boards.States;

namespace Veggerby.Boards.Go;

/// <summary>
/// Immutable Go-specific extra state captured per <see cref="GameState"/> snapshot.
/// </summary>
/// <param name="KoTileId">If set, tile identifier forbidden by ko this turn (simple ko only).</param>
/// <param name="ConsecutivePasses">Number of consecutive pass turns (0-2). Game ends at 2.</param>
/// <param name="BoardSize">Board dimension (e.g. 19 for 19x19). Immutable after game creation.</param>
public sealed record GoStateExtras(string KoTileId, int ConsecutivePasses, int BoardSize);