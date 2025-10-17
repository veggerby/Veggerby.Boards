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
    public static TilePath? ResolvePathCompiledFirst(this Game game, Piece? piece, Tile? from, Tile? to)
    {
        // Zero-length path requests are treated as no-op (null) – avoid constructing visitors which throw.
        if (from is null || to is null || piece is null)
        {
            return null;
        }
        if (from == to)
        {
            return null;
        }

        if (Internal.FeatureFlags.EnableCompiledPatterns && TryGetCompiledResolver(game, out var services) && services is not null && services.Resolver is not null)
        {
            if (services.Resolver.TryResolve(piece, from, to, out var compiledPath))
            {
                return compiledPath;
            }
        }

        // Legacy fallback: iterate each declared pattern until one yields a path (supports multi-direction sliders).
        // piece/from/to already validated non-null above

        foreach (var pat in piece.Patterns)
        {
            var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            pat.Accept(visitor);
            if (visitor.ResultPath is not null)
            {
                return visitor.ResultPath;
            }
        }

        return null;
    }

    private static bool TryGetCompiledResolver(Game game, out CompiledPatternServices? services)
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
    internal static bool TryGetCompiledResolver(this GameProgress? progress, out CompiledPatternServices? services)
    {
        services = null;
        if (!Internal.FeatureFlags.EnableCompiledPatterns)
        {
            return false;
        }

        if (progress?.Engine?.Capabilities is null)
        {
            return false;
        }
        // Compiled patterns no longer exposed directly on capabilities (resolver abstracted). Return false.
        return services is not null;
    }

    /// <summary>
    /// Resolves path preferring compiled patterns when available using progress context; falls back to legacy visitor.
    /// </summary>
    public static TilePath? ResolvePathCompiledFirst(this GameProgress? progress, Piece? piece, Tile? from, Tile? to)
    {
        if (progress is null || piece is null || from is null || to is null)
        {
            return null;
        }
        // Short-circuit zero-length requests for parity with legacy semantics.
        if (from == to)
        {
            return null;
        }

        // Single source of truth for fast-path attempt / skip / hit metrics.
        Internal.FastPathMetrics.OnAttempt();

        var capabilities = progress.Engine?.Capabilities;
        var accel = capabilities?.AccelerationContext;
        var topology = capabilities?.Topology;
        var pathResolver = capabilities?.PathResolver; // may be a SlidingFastPathResolver decorator

        bool pieceIsSlider = false;
        if (piece is not null)
        {
            foreach (var pat in piece.Patterns)
            {
                if (pat is Artifacts.Patterns.DirectionPattern dp && dp.IsRepeatable) { pieceIsSlider = true; break; }
                if (pat is Artifacts.Patterns.MultiDirectionPattern md && md.IsRepeatable) { pieceIsSlider = true; break; }
            }
        }

        TilePath? fastPath = null;
        var fastPathAttempted = false;
        // Fast-path reconstruction attempt (metrics gating centralized here).
        if (Internal.FeatureFlags.EnableSlidingFastPath && Internal.FeatureFlags.EnableBitboards && pieceIsSlider && accel?.AttackRays is not null && topology is not null)
        {
            fastPathAttempted = true;
            // piece/from/to proven non-null by earlier guard; null-forgiving silences analyzer while preserving runtime safety.
            fastPath = pathResolver?.Resolve(piece!, from!, to!, progress.State);
        }

        if (fastPathAttempted)
        {
            if (fastPath is not null)
            {
                Internal.FastPathMetrics.OnFastPathHit();
                return ApplyOccupancySemantics(progress, piece, fastPath);
            }
            else
            {
                // Distinguish skip reasons: not on ray / reconstruct fail accounted for by absence of path.
                // Currently we cannot differentiate AttackMiss vs ReconstructFail without extra signals; treat as attack miss.
                Internal.FastPathMetrics.OnFastPathSkipAttackMiss();
            }
        }
        else
        {
            if (!pieceIsSlider)
            {
                Internal.FastPathMetrics.OnFastPathSkipNotSlider();
            }
            else
            {
                Internal.FastPathMetrics.OnFastPathSkipNoServices();
            }
        }

        // Compiled or legacy resolution path after optional fast-path attempt
        if (piece is null || from is null || to is null)
        {
            return null;
        }
        if (progress.TryGetCompiledResolver(out var services) && services is not null && services.Resolver is not null && services.Resolver.TryResolve(piece!, from!, to!, out var compiledPath))
        {
            Internal.FastPathMetrics.OnCompiledHit();
            return ApplyOccupancySemantics(progress, piece, compiledPath);
        }

        var legacy = progress.Game.ResolvePathCompiledFirst(piece, from, to);
        if (legacy is not null)
        {
            Internal.FastPathMetrics.OnLegacyHit();
        }

        return ApplyOccupancySemantics(progress, piece, legacy);
    }

    private static TilePath? ApplyOccupancySemantics(GameProgress? progress, Piece? movingPiece, TilePath? path)
    {
        if (progress is null || movingPiece is null || path is null)
        {
            return null;
        }

        // Build occupancy (tile -> piece)
        var occupancy = new System.Collections.Generic.Dictionary<Tile, Piece>();
        foreach (var ps in progress.State.GetStates<PieceState>())
        {
            if (ps.CurrentTile is not null)
            {
                occupancy[ps.CurrentTile] = ps.Artifact;
            }
        }

        var relations = path.Relations.ToArray();
        for (int i = 0; i < relations.Length; i++)
        {
            var rel = relations[i];
            var isLast = i == relations.Length - 1;
            if (isLast)
            {
                if (occupancy.TryGetValue(rel.To, out var occ))
                {
                    if (occ.Owner?.Equals(movingPiece.Owner) == true)
                    {
                        return null; // cannot land on friendly piece
                    }
                }
            }
            else
            {
                if (occupancy.ContainsKey(rel.To))
                {
                    return null; // cannot pass through occupied square
                }
            }
        }

        return path;
    }
}