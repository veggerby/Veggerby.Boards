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
        foreach (var piece in game.Artifacts.OfType<Piece>())
        {
            var compiled = new List<CompiledPattern>();

            foreach (var pattern in piece.Patterns)
            {
                switch (pattern)
                {
                    case FixedPattern fixedPattern:
                        // Direct mapping: ordered steps -> Fixed compiled pattern
                        compiled.Add(CompiledPattern.Fixed(fixedPattern.Pattern.ToArray()));
                        break;
                    case MultiDirectionPattern multi:
                        // Map multi-direction movement to Ray/MultiRay respecting repeat flag
                        var dirs = multi.Directions.ToArray();
                        var repeat = multi.IsRepeatable;
                        if (dirs.Length == 1)
                        {
                            // Single direction degenerates to Ray kind
                            compiled.Add(CompiledPattern.Ray(dirs[0], repeat));
                        }
                        else if (dirs.Length > 1)
                        {
                            compiled.Add(CompiledPattern.MultiRay(dirs, repeat));
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
}