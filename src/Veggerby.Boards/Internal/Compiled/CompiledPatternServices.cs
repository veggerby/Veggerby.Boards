using Veggerby.Boards.Flows.Patterns;

namespace Veggerby.Boards.Internal.Compiled;

internal interface ICompiledPatternResolver
{
    bool TryResolve(Artifacts.Piece piece, Artifacts.Tile from, Artifacts.Tile to, out Artifacts.Relations.TilePath? path);
}

internal sealed class CompiledPatternServices(CompiledPatternTable table, ICompiledPatternResolver resolver, BoardAdjacencyCache adjacency)
{
    public CompiledPatternTable Table { get; } = table;
    public ICompiledPatternResolver Resolver { get; } = resolver;
    public BoardAdjacencyCache Adjacency { get; } = adjacency;
}