using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Attacks; // sliding attack fast-path
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
        // Sliding attack fast-path (bitboards + attack generator)
        if (Internal.FeatureFlags.EnableBitboards
            && progress?.Engine?.Services is not null
            && progress.Engine.Services.TryGet(out Internal.Attacks.AttackGeneratorServices atk)
            && progress.Engine.Services.TryGet(out Internal.Layout.BoardShape shape)
            && progress.Engine.Services.TryGet(out Internal.Layout.BitboardServices bb)
            && progress.Engine.Services.TryGet(out Internal.Layout.PieceMapServices pm))
        {
            if (shape.TryGetTileIndex(from, out var fromIdx) && shape.TryGetTileIndex(to, out var toIdx))
            {
                var attacks = atk.Sliding.GetSlidingAttacks(piece, (short)fromIdx, progress.PieceMapSnapshot, bb.Snapshot);
                if (attacks.Contains((short)toIdx))
                {
                    // Determine direction: find first direction whose ray sequence reaches target via consecutive neighbors
                    var direction = shape.Directions.FirstOrDefault(d =>
                    {
                        var current = from;
                        while (shape.TryGetNeighbor(current, d, out var n))
                        {
                            if (n.Equals(to)) { return true; }
                            current = n;
                        }

                        return false;
                    });

                    if (direction is not null)
                    {
                        TilePath built = null;
                        var current = from;
                        while (!current.Equals(to))
                        {
                            if (!shape.TryGetNeighbor(current, direction, out var n)) { built = null; break; }
                            var rel = progress.Game.Board.GetTileRelation(current, direction);
                            built = TilePath.Create(built, rel);
                            current = n;
                        }

                        if (built is not null && built.To.Equals(to))
                        {
                            return built; // fast-path win
                        }
                    }
                }
            }
        }

        if (progress.TryGetCompiledResolver(out var services))
        {
            if (services.Resolver.TryResolve(piece, from, to, out var compiledPath))
            {
                return ApplyOccupancySemantics(progress, piece, compiledPath);
            }
        }

        var legacy = progress.Game.ResolvePathCompiledFirst(piece, from, to);
        return ApplyOccupancySemantics(progress, piece, legacy);
    }

    private static TilePath ApplyOccupancySemantics(GameProgress progress, Piece movingPiece, TilePath path)
    {
        if (path is null)
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

        var rels = path.Relations.ToArray();
        for (int i = 0; i < rels.Length; i++)
        {
            var rel = rels[i];
            var isLast = i == rels.Length - 1;
            if (isLast)
            {
                if (occupancy.TryGetValue(rel.To, out var occ))
                {
                    if (occ.Owner.Equals(movingPiece.Owner))
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