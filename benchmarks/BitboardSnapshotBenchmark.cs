using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures cost of building bitboard + piece map snapshots (initial vs incremental after a single move).
/// Focus: capture allocation profile and relative cost to inform future pooling or incremental diff strategies.
/// </summary>
[MemoryDiagnoser]
public class BitboardSnapshotBenchmark
{
    private Game _game = null!;
    private BoardShape _shape = null!;
    private GameState _initialState = null!;
    private GameState _postMoveState = null!;
    private BitboardLayout _bbLayout = null!;
    private PieceMapLayout _pieceLayout = null!;

    [GlobalSetup]
    public void Setup()
    {
        // simple 8x8 like chess (only orthogonal for brevity; diagonals not required for snapshot building)
        var north = new Direction(Constants.Directions.North); var south = new Direction(Constants.Directions.South); var east = new Direction(Constants.Directions.East); var west = new Direction(Constants.Directions.West);
        var tiles = new List<Tile>();
        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        for (int r = 1; r <= 8; r++)
        {
            foreach (var f in files)
            {
                tiles.Add(new Tile($"tile-{f}{r}"));
            }
        }
        Tile T(char f, int r) => tiles.Single(t => t.Id == $"tile-{f}{r}");
        var relations = new List<TileRelation>();
        foreach (var f in files)
        {
            for (int r = 1; r <= 8; r++)
            {
                if (r < 8) relations.Add(new TileRelation(T(f, r), T(f, r + 1), north));
                if (r > 1) relations.Add(new TileRelation(T(f, r), T(f, r - 1), south));
                if (f < 'h') relations.Add(new TileRelation(T(f, r), T((char)(f + 1), r), east));
                if (f > 'a') relations.Add(new TileRelation(T(f, r), T((char)(f - 1), r), west));
            }
        }
        var board = new Board("bitboard-bench-8x8", relations);
        var white = new Player("white"); var black = new Player("black");
        // create a handful of pieces
        var movers = new List<Piece>();
        for (int i = 0; i < 16; i++)
        {
            movers.Add(new Piece($"w{i}", white, new[] { new NullPattern() }));
            movers.Add(new Piece($"b{i}", black, new[] { new NullPattern() }));
        }
        var artifacts = new List<Artifact>();
        artifacts.Add(board); artifacts.Add(white); artifacts.Add(black); artifacts.AddRange(movers);
        _game = new Game(board, new[] { white, black }, artifacts);
        _shape = BoardShape.Build(board);

        // place pieces: white first two ranks, black last two ranks (like chess pawns+backline simplified)
        var states = new List<IArtifactState>();
        int index = 0;
        foreach (var p in movers.Where(p => p.Owner == white).Take(16))
        {
            int file = index % 8; int rank = index < 8 ? 1 : 2;
            states.Add(new PieceState(p, T(files[file], rank)));
            index++;
        }
        index = 0;
        foreach (var p in movers.Where(p => p.Owner == black).Take(16))
        {
            int file = index % 8; int rank = index < 8 ? 7 : 8;
            states.Add(new PieceState(p, T(files[file], rank)));
            index++;
        }
        _initialState = GameState.New(states);
        // simulate a single move: move one white piece forward one rank
        var movedStates = new List<IArtifactState>();
        foreach (var s in states)
        {
            if (s is PieceState ps && ps.CurrentTile.Id == "tile-a2")
            {
                movedStates.Add(new PieceState(ps.Artifact, T('a', 3)));
            }
            else
            {
                movedStates.Add(s);
            }
        }
        _postMoveState = GameState.New(movedStates);

        _bbLayout = BitboardLayout.Build(_game);
        _pieceLayout = PieceMapLayout.Build(_game);
    }

    [Benchmark(Baseline = true)]
    public (int pieceCount, int playerCount) InitialBuild()
    {
        var pieceSnapshot = PieceMapSnapshot.Build(_pieceLayout, _initialState, _shape);
        var bbSnapshot = BitboardSnapshot.Build(_bbLayout, _initialState, _shape);
        // expose counts from layouts (snapshot types internal) to prevent elimination
        return (_pieceLayout.PieceCount, _bbLayout.PlayerCount);
    }

    [Benchmark]
    public (int pieceCount, int playerCount) PostMoveBuild()
    {
        var pieceSnapshot = PieceMapSnapshot.Build(_pieceLayout, _postMoveState, _shape);
        var bbSnapshot = BitboardSnapshot.Build(_bbLayout, _postMoveState, _shape);
        return (_pieceLayout.PieceCount, _bbLayout.PlayerCount);
    }
}