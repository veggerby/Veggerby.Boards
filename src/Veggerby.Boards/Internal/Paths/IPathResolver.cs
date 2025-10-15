using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Internal.Paths;

/// <summary>
/// Provides movement path resolution for a piece between two tiles, abstracting underlying strategy
/// (sliding fast-path, compiled patterns, legacy visitor fallback).
/// </summary>
/// <remarks>
/// Implementations must be pure and deterministic: same inputs (piece, from, to, state snapshot) produce identical outputs.
/// All returned <see cref="TilePath"/> instances must reflect valid geometry honoring occupancy semantics.
/// </remarks>
internal interface IPathResolver
{
    /// <summary>
    /// Attempts to resolve a movement path for the specified piece from an origin to a target tile.
    /// </summary>
    /// <param name="piece">Piece being moved.</param>
    /// <param name="from">Origin tile.</param>
    /// <param name="to">Destination tile.</param>
    /// <param name="state">Current immutable game state snapshot.</param>
    /// <returns>A <see cref="TilePath"/> if resolvable; otherwise <c>null</c>.</returns>
    TilePath? Resolve(Piece piece, Tile from, Tile to, States.GameState state);
}