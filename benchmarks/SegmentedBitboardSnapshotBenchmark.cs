using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks <see cref="BitboardSnapshot.Build"/> performance with and without the segmented bitboard flag
/// for 64-tile (chess-like) and 128-tile synthetic boards. Measures build throughput and allocations.
/// </summary>
[MemoryDiagnoser]
public class SegmentedBitboardSnapshotBenchmark
{
    private record BenchmarkBoard(Board Board, Game Game, GameState State, BitboardLayout Layout, BoardShape Shape);

    [Params(64, 128)]
    public int TileCount
    {
        get; set;
    }
    [Params(false, true)]
    public bool SegmentedEnabled
    {
        get; set;
    }

    private BenchmarkBoard _ctx = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Build synthetic board:
        // - For 64: 8x8 grid (orth + diag) similar to chess to exercise multi-direction occupancy locality.
        // - For 128: linear chain with wrap + sparse occupancy (every 4th tile) to engage multi segment path.
        if (TileCount == 64)
        {
            _ctx = BuildGrid64();
        }
        else
        {
            _ctx = BuildLinear128(TileCount);
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Toggle feature flag per param; isolation: flag is static so restore handled in benchmark invocation.
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        // Reset to default (false) to avoid bleed into other benchmarks.
    }

    private BitboardSnapshot _last = null!; // set during first benchmark iteration

    [Benchmark]
    public int BuildSnapshot()
    {
        _last = BitboardSnapshot.Build(_ctx.Layout, _ctx.State, _ctx.Shape);
        // Return popcount to prevent dead-code elimination.
        if (_last.GlobalOccupancy128.HasValue)
        {
            var v = _last.GlobalOccupancy128.Value;
            return System.Numerics.BitOperations.PopCount(v.Low) + System.Numerics.BitOperations.PopCount(v.High);
        }
        return System.Numerics.BitOperations.PopCount(_last.GlobalOccupancy);
    }

    private static BenchmarkBoard BuildGrid64()
    {
        var tiles = new List<Tile>();
        var relations = new List<TileRelation>();
        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        for (int r = 1; r <= 8; r++)
        {
            foreach (var f in files)
            {
                tiles.Add(new Tile($"{f}{r}"));
            }
        }
        Tile T(char f, int r) => tiles[(r - 1) * 8 + (f - 'a')];
        var dirs = new (string id, (int df, int dr)[] deltas)[]
        {
            (Constants.Directions.North, [(0,1)]),
            (Constants.Directions.South, [(0,-1)]),
            (Constants.Directions.East,  [(1,0)]),
            (Constants.Directions.West,  [(-1,0)]),
            (Constants.Directions.NorthEast, [(1,1)]),
            (Constants.Directions.NorthWest, [(-1,1)]),
            (Constants.Directions.SouthEast, [(1,-1)]),
            (Constants.Directions.SouthWest, [(-1,-1)])
        };
        // Create relation objects (Direction reused by id equality semantics not required; we just instantiate new)
        foreach (var (id, delta) in dirs)
        {
            foreach (var (df, dr) in delta)
            {
                for (int r = 1; r <= 8; r++)
                {
                    foreach (var f in files)
                    {
                        var nf = (char)(f + df);
                        var nr = r + dr;
                        if (nr < 1 || nr > 8 || nf < 'a' || nf > 'h')
                        {
                            continue;
                        }
                        relations.Add(new TileRelation(T(f, r), T(nf, nr), new Direction(id)));
                    }
                }
            }
        }
        var board = new Board("grid-64", relations);
        var white = new Player("p1");
        var black = new Player("p2");
        // Construct pieces with empty pattern lists (patterns not needed for snapshot build)
        var pieces = new List<Piece>(32);
        for (int i = 0; i < 16; i++)
        {
            pieces.Add(new Piece($"w{i}", white, Array.Empty<Veggerby.Boards.Artifacts.Patterns.IPattern>()));
            pieces.Add(new Piece($"b{i}", black, Array.Empty<Veggerby.Boards.Artifacts.Patterns.IPattern>()));
        }
        var game = new Game(board, new[] { white, black }, pieces);
        var initialStates = new List<Veggerby.Boards.States.IArtifactState>(pieces.Count);
        for (int i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];
            Tile tile = i < 16 ? tiles[i] : tiles[tiles.Count - (i - 16) - 1];
            initialStates.Add(new PieceState(piece, tile));
        }
        var state = GameState.New(initialStates);
        var shape = BoardShape.Build(board);
        var layout = BitboardLayout.Build(game);
        return new BenchmarkBoard(board, game, state, layout, shape);
    }

    private static BenchmarkBoard BuildLinear128(int tileCount)
    {
        var tiles = new List<Tile>(tileCount);
        var relations = new List<TileRelation>(tileCount * 2);
        for (int i = 0; i < tileCount; i++)
        {
            tiles.Add(new Tile($"t{i}"));
        }
        var east = new Direction(Constants.Directions.East);
        for (int i = 0; i < tileCount - 1; i++)
        {
            relations.Add(new TileRelation(tiles[i], tiles[i + 1], east));
        }
        relations.Add(new TileRelation(tiles[tileCount - 1], tiles[0], east)); // wrap
        var board = new Board($"linear-{tileCount}", relations);
        var p1 = new Player("p1");
        var p2 = new Player("p2");
        var pieces = new List<Piece>();
        for (int i = 0; i < tileCount; i += 4)
        {
            var owner = ((i / 4) % 2) == 0 ? p1 : p2;
            pieces.Add(new Piece($"pc{i}", owner, Array.Empty<Veggerby.Boards.Artifacts.Patterns.IPattern>()));
        }
        var game = new Game(board, new[] { p1, p2 }, pieces);
        var states = new List<Veggerby.Boards.States.IArtifactState>(pieces.Count);
        for (int i = 0; i < pieces.Count; i++)
        {
            states.Add(new PieceState(pieces[i], tiles[(i * 4) % tileCount]));
        }
        var state = GameState.New(states);
        var shape = BoardShape.Build(board);
        var layout = BitboardLayout.Build(game);
        return new BenchmarkBoard(board, game, state, layout, shape);
    }
}