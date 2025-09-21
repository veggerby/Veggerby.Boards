using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Internal.Compiled;

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
        // resolver stored on engine services; walk through any associated engines in progress not available here – using reflection not desired
        // For now: compiled services not directly accessible from Game; feature limited to internal usage via GameProgress.Engine
        services = null; return false;
    }
}