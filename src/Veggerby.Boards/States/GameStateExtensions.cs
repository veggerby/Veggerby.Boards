using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Convenience helpers for querying a <see cref="GameState"/>.
/// </summary>
public static class GameStateExtensions
{
    /// <summary>
    /// Tries to get the single active player for the supplied game state without throwing.
    /// </summary>
    /// <param name="gameState">The game state.</param>
    /// <param name="activePlayer">When this method returns, contains the active player if exactly one is present; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> when exactly one active player is present; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// This helper avoids exceptions when zero or multiple active players exist, simplifying conditions and guards.
    /// </remarks>
    public static bool TryGetActivePlayer(this GameState gameState, out Player? activePlayer)
    {
        activePlayer = null;
        var seen = false;
        foreach (var aps in gameState.GetStates<ActivePlayerState>())
        {
            if (!aps.IsActive)
            {
                continue;
            }

            if (!seen)
            {
                activePlayer = aps.Artifact;
                seen = true;
            }
            else
            {
                // More than one active -> ambiguous
                activePlayer = null;
                return false;
            }
        }

        return seen;
    }

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
    public static IEnumerable<Piece> GetPiecesOnTile(this GameState gameState, Tile tile, Player? owner = null)
    {
        // Only material piece states (exclude captured)
        return [.. gameState
            .GetStates<PieceState>()
            .Where(x => x.CurrentTile.Equals(tile) && (owner is null || x.Artifact.Owner.Equals(owner)))
            .Select(x => x.Artifact)];
    }

    /// <summary>
    /// Gets the captured state for a specific piece or null if the piece is not captured.
    /// </summary>
    public static CapturedPieceState? GetCapturedState(this GameState gameState, Piece piece)
    {
        return gameState.GetStates<CapturedPieceState>().FirstOrDefault(s => s.Artifact.Equals(piece));
    }

    /// <summary>
    /// Returns true if the supplied piece has a captured state present in this game state.
    /// </summary>
    public static bool IsCaptured(this GameState gameState, Piece piece) => gameState.GetCapturedState(piece) is not null;

    /// <summary>
    /// Retrieves a previously registered extras state (added via GameBuilder.WithState) or null when absent.
    /// </summary>
    /// <typeparam name="T">Extras record type.</typeparam>
    /// <param name="gameState">Game state.</param>
    /// <returns>Extras instance or null.</returns>
    public static T? GetExtras<T>(this GameState gameState) where T : class
    {
        // Find matching generic ExtrasState<T>
        foreach (var state in gameState.ChildStates)
        {
            if (state.GetType().IsGenericType && state.GetType().GetGenericTypeDefinition() == typeof(ExtrasState<>) && state is IArtifactState)
            {
                if (state.GetType().GetGenericArguments()[0] == typeof(T))
                {
                    var prop = state.GetType().GetProperty("Value");
                    return prop?.GetValue(state) as T;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Replaces (or adds) an extras state value for the supplied type producing a successor state.
    /// </summary>
    public static GameState ReplaceExtras<T>(this GameState gameState, T value) where T : class
    {
        Artifact? artifact = null;
        foreach (var state in gameState.ChildStates)
        {
            if (state.GetType().IsGenericType && state.GetType().GetGenericTypeDefinition() == typeof(ExtrasState<>) && state.GetType().GetGenericArguments()[0] == typeof(T))
            {
                artifact = state.Artifact;
                break;
            }
        }
        artifact ??= new ExtrasArtifact($"extras-{typeof(T).FullName}");
        var wrapper = new ExtrasState<T>(artifact, value);
        return gameState.Next([wrapper]);
    }
}