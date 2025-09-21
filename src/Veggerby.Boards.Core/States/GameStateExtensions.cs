using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

/// <summary>
/// Convenience helpers for querying a <see cref="GameState"/>.
/// </summary>
public static class GameStateExtensions
{
    /// <summary>
    /// Gets the single active player for the supplied game state.
    /// </summary>
    /// <param name="gameState">The game state.</param>
    /// <returns>The active player artifact.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when zero or multiple active players are present.</exception>
    public static Player GetActivePlayer(this GameState gameState)
    {
        return gameState
            .GetStates<ActivePlayerState>()
            .Where(x => x.IsActive)
            .Select(x => x.Artifact)
            .Single();
    }

    /// <summary>
    /// Gets all pieces currently located on the specified tile, optionally filtered by owner.
    /// </summary>
    /// <param name="gameState">The game state.</param>
    /// <param name="tile">The tile to inspect.</param>
    /// <param name="owner">Optional owner filter.</param>
    /// <returns>Enumeration of piece artifacts.</returns>
    public static IEnumerable<Piece> GetPiecesOnTile(this GameState gameState, Tile tile, Player owner = null)
    {
        return [.. gameState
            .GetStates<PieceState>()
            .Where(x => x.CurrentTile == tile && (owner is null || (x.Artifact).Owner.Equals(owner)))
            .Select(x => x.Artifact)];
    }
}