using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Microbenchmarks for identity lookups: piece/tile retrieval and iterating piece states.
/// Helps detect accidental regressions in hash/equality or state enumeration paths.
/// </summary>
[MemoryDiagnoser]
public class HashingLookupBenchmark
{
    private Game _game = null!;
    private GameState _state = null!;
    private Piece[] _whitePieces = null!;
    private Tile[] _tiles = null!;

    [Params(32, 64, 96)] public int PieceCount { get; set; } = 64; // vary number of pieces to simulate different densities

    [GlobalSetup]
    public void Setup()
    {
        // Build simple linear board (PieceCount * 2 tiles) with a minimal chain of relations so Board validation passes.
        // Relations are not semantically used by the benchmarks (pure identity lookups) but required for Board construction.
        var tiles = new List<Tile>();
        for (int i = 0; i < PieceCount * 2; i++)
        {
            tiles.Add(new Tile($"t{i}"));
        }

        // Single synthetic direction reused for all linear relations.
        var forward = new Direction("forward");
        var relations = new List<TileRelation>();
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            relations.Add(new TileRelation(tiles[i], tiles[i + 1], forward));
        }

        var board = new Board($"hash-bench-{PieceCount}", relations);
        var white = new Player("white"); var black = new Player("black");
        var artifacts = new List<Artifact> { board, white, black };
        var whitePieces = new List<Piece>();
        var blackPieces = new List<Piece>();
        for (int i = 0; i < PieceCount / 2; i++)
        {
            whitePieces.Add(new Piece($"w{i}", white, new[] { new NullPattern() }));
            blackPieces.Add(new Piece($"b{i}", black, new[] { new NullPattern() }));
        }
        artifacts.AddRange(whitePieces);
        artifacts.AddRange(blackPieces);
        _game = new Game(board, new[] { white, black }, artifacts);
        _tiles = tiles.ToArray();
        _whitePieces = whitePieces.ToArray();
        var states = new List<IArtifactState>();
        int tileCursor = 0;
        foreach (var p in whitePieces)
        {
            states.Add(new PieceState(p, tiles[tileCursor++]));
        }
        foreach (var p in blackPieces)
        {
            states.Add(new PieceState(p, tiles[tileCursor++]));
        }
        _state = GameState.New(states);
    }

    [Benchmark(Baseline = true)]
    public int EnumeratePieceStates()
    {
        int count = 0;
        foreach (var ps in _state.GetStates<PieceState>())
        {
            if (ps.Artifact.Owner.Id == "white") count++;
        }
        return count;
    }

    [Benchmark]
    public int LookupPiecesById()
    {
        int hits = 0;
        foreach (var piece in _whitePieces)
        {
            var resolved = _game.GetPiece(piece.Id);
            if (resolved is not null) hits++;
        }
        return hits;
    }

    [Benchmark]
    public int ResolveTilesSequentially()
    {
        int hits = 0;
        foreach (var tile in _tiles)
        {
            var resolved = _game.GetTile(tile.Id);
            if (resolved is not null) hits++;
        }
        return hits;
    }
}