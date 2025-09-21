using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Compiled;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Extensions bridging compiled pattern resolution with legacy visitor fallback.
/// </summary>
public static partial class GameExtensions
{
    /// <summary>
    /// Attempts to resolve a path using compiled patterns (when enabled) otherwise falls back to visitor-based pattern resolution.
    /// </summary>
    public static TilePath ResolvePathCompiledFirst(this Game game, Piece piece, Tile from, Tile to)
    {
        if (Internal.FeatureFlags.EnableCompiledPatterns && TryGetCompiledResolver(game, out var services))
        {
            if (services.Resolver.TryResolve(piece, from, to, out var compiledPath))
            {
                return compiledPath;
            }
        }

        // Legacy fallback (single pattern assumption retained)
        var pattern = piece.Patterns.Count() == 1 ? piece.Patterns.First() : null; // minimal parity with prior Single() usage
        if (pattern is null)
        {
            return null;
        }

        var visitor = new Artifacts.Relations.ResolveTilePathPatternVisitor(game.Board, from, to);
        pattern.Accept(visitor);
        return visitor.ResultPath;
    }

    private static bool TryGetCompiledResolver(Game game, out CompiledPatternServices services)
    {
        services = null;
        if (game is null)
        {
            return false;
        }

        // Attempt to locate an active progress via heuristic: game has no direct back-reference to engine for purity reasons.
        // Compiled path is primarily used during live progress operations, so expose a hook via GameProgress extension instead.
        return false;
    }
}

/// <summary>
/// Extensions operating on <see cref="GameProgress"/> to leverage compiled pattern services (engine access available).
/// </summary>
public static partial class GameExtensions
{
    /// <summary>
    /// Attempts to retrieve compiled pattern services from a progress (engine-bound) context.
    /// </summary>
    internal static bool TryGetCompiledResolver(this GameProgress progress, out CompiledPatternServices services)
    {
        services = null;
        if (!Internal.FeatureFlags.EnableCompiledPatterns)
        {
            return false;
        }
        if (progress?.Engine?.Services is null)
        {
            return false;
        }
        return progress.Engine.Services.TryGet(out services);
    }

    /// <summary>
    /// Resolves path preferring compiled patterns when available using progress context; falls back to legacy visitor.
    /// </summary>
    public static TilePath ResolvePathCompiledFirst(this GameProgress progress, Piece piece, Tile from, Tile to)
    {
        if (progress.TryGetCompiledResolver(out var services))
        {
            if (services.Resolver.TryResolve(piece, from, to, out var compiledPath))
            {
                return compiledPath;
            }
        }
        return progress.Game.ResolvePathCompiledFirst(piece, from, to);
    }
}