using System.Diagnostics.CodeAnalysis;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Path resolution extension methods providing Try-pattern alternatives to nullable-return convenience methods.
/// </summary>
public static partial class GameExtensions
{
    /// <summary>
    /// Attempts to resolve a path from one tile to another for the given piece, using compiled patterns when available.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <param name="piece">The piece being moved.</param>
    /// <param name="from">The origin tile.</param>
    /// <param name="to">The destination tile.</param>
    /// <param name="path">When this method returns, contains the resolved path if successful; otherwise, null.</param>
    /// <returns><c>true</c> if a legal path was resolved; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Prefer this method over <see cref="ResolvePathCompiledFirst(Game, Piece?, Tile?, Tile?)"/> when chaining multiple resolution attempts
    /// or when the caller needs to distinguish between "no path found" and other failure modes without additional null checks.
    /// </remarks>
    public static bool TryResolvePath(this Game game, Piece piece, Tile from, Tile to, [NotNullWhen(true)] out TilePath? path)
    {
        path = game.ResolvePathCompiledFirst(piece, from, to);
        return path is not null;
    }

    /// <summary>
    /// Attempts to resolve a path from one tile to another for the given piece in the context of a game progress, using compiled patterns when available.
    /// </summary>
    /// <param name="progress">The game progress instance.</param>
    /// <param name="piece">The piece being moved.</param>
    /// <param name="from">The origin tile.</param>
    /// <param name="to">The destination tile.</param>
    /// <param name="path">When this method returns, contains the resolved path if successful; otherwise, null.</param>
    /// <returns><c>true</c> if a legal path was resolved; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Prefer this method over <see cref="ResolvePathCompiledFirst(GameProgress?, Piece?, Tile?, Tile?)"/> when chaining multiple resolution attempts
    /// or when the caller needs to distinguish between "no path found" and other failure modes without additional null checks.
    /// </remarks>
    public static bool TryResolvePath(this GameProgress progress, Piece piece, Tile from, Tile to, [NotNullWhen(true)] out TilePath? path)
    {
        path = progress.ResolvePathCompiledFirst(piece, from, to);
        return path is not null;
    }
}
