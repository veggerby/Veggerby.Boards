using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Placeholder microbenchmark for movement pattern path resolution (legacy visitor vs compiled resolver).
/// </summary>
/// <remarks>
/// The compiled pattern compiler currently emits an empty table (population pending). This benchmark scaffolds
/// the harness so that once <see cref="PatternCompiler"/> is populated we can immediately begin measuring
/// relative resolution performance. Until then, the compiled path will always be <c>null</c>.
/// </remarks>
[MemoryDiagnoser]
public class PatternResolutionBaseline
{
    private Game _game = null!;
    private Piece _piece = null!;
    private Tile _from = null!;
    private Tile _to = null!;
    private CompiledPatternResolver _compiledResolver = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Minimal linear board: a -> b -> c -> d -> e
        var a = new Tile("a");
        var b = new Tile("b");
        var c = new Tile("c");
        var d = new Tile("d");
        var e = new Tile("e");
        var d1 = new Direction("ab");
        var d2 = new Direction("bc");
        var d3 = new Direction("cd");
        var d4 = new Direction("de");
        var r1 = new TileRelation(a, b, d1);
        var r2 = new TileRelation(b, c, d2);
        var r3 = new TileRelation(c, d, d3);
        var r4 = new TileRelation(d, e, d4);
        var board = new Board("board-pattern-benchmark", new[] { r1, r2, r3, r4 });
        var player = new Player("p1");
        _piece = new Piece("piece-1", player, new IPattern[] { new FixedPattern(new[] { d1, d2, d3, d4 }) });
        _game = new Game(board, new[] { player }, new Artifact[] { _piece });
        _from = a; _to = e;

        // Pre-create compiled resolver (currently empty table -> always miss)
        var table = PatternCompiler.Compile(_game);
        _compiledResolver = new CompiledPatternResolver(table, _game.Board);
    }

    [Benchmark(Baseline = true)]
    public TilePath Resolve_Visitor()
    {
        var visitor = new ResolveTilePathPatternVisitor(_game.Board, _from, _to);
        _piece.Patterns.First().Accept(visitor);
        return visitor.ResultPath; // expected non-null
    }

    [Benchmark]
    public TilePath Resolve_Compiled()
    {
        _compiledResolver.TryResolve(_piece, _from, _to, out var path);
        return path; // currently null until compiler populated
    }
}

/// <summary>
/// Program entry to execute the pattern resolution benchmark in isolation.
/// </summary>
public static class PatternResolutionBaselineProgram
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<PatternResolutionBaseline>();
    }
}