using Veggerby.Boards.Flows.Patterns;

namespace Veggerby.Boards.Internal.Compiled;

internal interface ICompiledPatternResolver
{
    bool TryResolve(Artifacts.Piece piece, Artifacts.Tile from, Artifacts.Tile to, out Artifacts.Relations.TilePath path);
}

internal sealed class CompiledPatternServices
{
    public CompiledPatternTable Table { get; }
    public ICompiledPatternResolver Resolver { get; }
    public BoardAdjacencyCache Adjacency { get; }

    public CompiledPatternServices(CompiledPatternTable table, ICompiledPatternResolver resolver, BoardAdjacencyCache adjacency)
    {
        Table = table;
        Resolver = resolver;
        Adjacency = adjacency;
    }
}