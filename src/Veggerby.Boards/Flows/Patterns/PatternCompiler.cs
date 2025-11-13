using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;

namespace Veggerby.Boards.Flows.Patterns;

/// <summary>
/// Compiles high-level <see cref="IPattern"/> instances into a simplified IR for fast runtime resolution.
/// </summary>
internal static class PatternCompiler
{
    public static CompiledPatternTable Compile(Game game)
    {
        var table = new CompiledPatternTable();
        // Cache identical direction sequences to avoid repeated array allocations.
        var directionSequenceCache = new Dictionary<string, Veggerby.Boards.Artifacts.Relations.Direction[]>();

        foreach (var piece in game.Artifacts.OfType<Piece>())
        {
            // Capacity hint: worst case each pattern compiles to one compiled pattern
            var patternCount = piece.Patterns is ICollection<IPattern> coll ? coll.Count : piece.Patterns.Count();
            var compiled = new List<CompiledPattern>(patternCount);

            foreach (var pattern in piece.Patterns)
            {
                switch (pattern)
                {
                    case FixedPattern fixedPattern:
                        // Direct mapping: ordered steps -> Fixed compiled pattern (cache)
                        var fixedDirs = fixedPattern.Pattern.ToArray();
                        var fixedKey = BuildKey(fixedDirs);
                        if (!directionSequenceCache.TryGetValue(fixedKey, out var cachedFixed))
                        {
                            cachedFixed = fixedDirs.Length == 0 ? System.Array.Empty<Veggerby.Boards.Artifacts.Relations.Direction>() : fixedDirs;
                            directionSequenceCache[fixedKey] = cachedFixed;
                        }

                        compiled.Add(CompiledPattern.Fixed(cachedFixed));
                        break;
                    case DirectionPattern singleDir:
                        // Single direction -> Ray (repeatable flag mirrors pattern)
                        compiled.Add(CompiledPattern.Ray(singleDir.Direction, singleDir.IsRepeatable));
                        break;
                    case MultiDirectionPattern multi:
                        // Map multi-direction movement to Ray/MultiRay respecting repeat flag (cache array)
                        var multiDirsRaw = multi.Directions.ToArray();
                        var repeat = multi.IsRepeatable;
                        if (multiDirsRaw.Length == 1)
                        {
                            compiled.Add(CompiledPattern.Ray(multiDirsRaw[0], repeat));
                        }
                        else if (multiDirsRaw.Length > 1)
                        {
                            var multiKey = BuildKey(multiDirsRaw);
                            if (!directionSequenceCache.TryGetValue(multiKey, out var cachedMulti))
                            {
                                cachedMulti = multiDirsRaw;
                                directionSequenceCache[multiKey] = cachedMulti;
                            }

                            compiled.Add(CompiledPattern.MultiRay(cachedMulti, repeat));
                        }

                        break;
                    case AnyPattern:
                        // Wildcard pattern carries no concrete movement; cannot compile deterministically.
                        break;
                    case NullPattern:
                        // No movement; ignore.
                        break;
                    default:
                        // Future pattern types can be added here without breaking existing compilation.
                        break;
                }
            }

            table.Add(new CompiledPiecePatterns(piece.Id, compiled));
        }
        return table;
    }

    private static string BuildKey(Veggerby.Boards.Artifacts.Relations.Direction[] dirs)
    {
        if (dirs.Length == 0)
        {
            return string.Empty;
        }

        // Build simple hash key: dir.Id concatenation with '|'. Direction.Id assumed deterministic.
        // Avoid StringBuilder for small arrays (pattern counts typically small); fallback to builder when > 8.
        if (dirs.Length <= 8)
        {
            var key = dirs[0].Id;
            for (var i = 1; i < dirs.Length; i++)
            {
                key += "|" + dirs[i].Id;
            }
            return key;
        }

        var sb = new System.Text.StringBuilder(dirs[0].Id);
        for (var i = 1; i < dirs.Length; i++)
        {
            sb.Append('|').Append(dirs[i].Id);
        }

        return sb.ToString();
    }
}