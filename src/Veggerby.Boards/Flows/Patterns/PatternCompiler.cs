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
            // Minimal phase: derive from piece movement metadata if available; currently we inspect attached patterns via reflection/known interfaces.
            // Placeholder: until piece exposes patterns, skip (empty) to allow future integration without breaking.
            table.Add(new CompiledPiecePatterns(piece.Id, compiled));
        }
        return table;
    }
}