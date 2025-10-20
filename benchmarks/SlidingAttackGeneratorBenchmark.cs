using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks sliding attack generator build performance for different board topologies:
/// 1. Linear single-direction ring (degenerate) - expected neutralization.
/// 2. Orthogonal 8x8 grid (rook style richness).
/// 3. Orthogonal+Diagonal 8x8 grid (queen style richness).
/// </summary>
[MemoryDiagnoser]
public class SlidingAttackGeneratorBenchmark
{
    [Params("ring-128", "grid-orth", "grid-queen")] public string Topology { get; set; } = "ring-128";

    private BoardShape _shape = null!;

    [GlobalSetup]
    public void Setup()
    {
        _shape = Topology switch
        {
            "ring-128" => BuildRing(128),
            "grid-orth" => BuildGrid(orthogonalOnly: true),
            "grid-queen" => BuildGrid(orthogonalOnly: false),
            _ => BuildRing(128)
        };
    }

    [Benchmark]
    public object BuildSliding()
    {
        return SlidingAttackGenerator.Build(_shape);
    }

    private static BoardShape BuildRing(int n)
    {
        var tiles = new List<Tile>(n);
        for (int i = 0; i < n; i++)
            tiles.Add(new Tile($"t{i}"));
        var east = new Direction("east");
        var rels = new List<TileRelation>(n);
        for (int i = 0; i < n - 1; i++)
            rels.Add(new TileRelation(tiles[i], tiles[i + 1], east));
        rels.Add(new TileRelation(tiles[n - 1], tiles[0], east)); // wrap
        var board = new Board($"ring-{n}", rels);
        return BoardShape.Build(board);
    }

    private static BoardShape BuildGrid(bool orthogonalOnly)
    {
        var tiles = new List<Tile>(64);
        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        for (int r = 1; r <= 8; r++)
            foreach (var f in files)
                tiles.Add(new Tile($"{f}{r}"));
        Tile T(char f, int r) => tiles[(r - 1) * 8 + (f - 'a')];
        var rels = new List<TileRelation>();
        var north = new Direction("north");
        var south = new Direction("south");
        var east = new Direction("east");
        var west = new Direction("west");
        var ne = new Direction("ne");
        var nw = new Direction("nw");
        var se = new Direction("se");
        var sw = new Direction("sw");
        foreach (var f in files)
        {
            for (int r = 1; r <= 8; r++)
            {
                if (r < 8)
                    rels.Add(new TileRelation(T(f, r), T(f, r + 1), north));
                if (r > 1)
                    rels.Add(new TileRelation(T(f, r), T(f, r - 1), south));
                if (f != 'h')
                    rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r), east));
                if (f != 'a')
                    rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r), west));
                if (!orthogonalOnly)
                {
                    if (r < 8 && f != 'h')
                        rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r + 1), ne));
                    if (r < 8 && f != 'a')
                        rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r + 1), nw));
                    if (r > 1 && f != 'h')
                        rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r - 1), se));
                    if (r > 1 && f != 'a')
                        rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r - 1), sw));
                }
            }
        }
        var board = new Board(orthogonalOnly ? "grid-orth" : "grid-queen", rels);
        return BoardShape.Build(board);
    }
}