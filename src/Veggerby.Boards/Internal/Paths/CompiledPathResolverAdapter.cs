using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Compiled;

namespace Veggerby.Boards.Internal.Paths;

/// <summary>
/// Adapts the existing compiled pattern resolver to the generic <see cref="IPathResolver"/> interface.
/// </summary>
internal sealed class CompiledPathResolverAdapter(ICompiledPatternResolver compiled) : IPathResolver
{
    private readonly ICompiledPatternResolver _compiled = compiled;

    public TilePath Resolve(Piece piece, Tile from, Tile to, States.GameState state)
    {
        if (piece is null || from is null || to is null)
        {
            return null;
        }

        if (_compiled is null)
        {
            return null;
        }

        return _compiled.TryResolve(piece, from, to, out var path) ? path : null;
    }
}