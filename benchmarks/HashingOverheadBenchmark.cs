using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks relative overhead of state hashing (64 + 128 bit) during HandleEvent flow.
/// </summary>
/// <remarks>
/// Uses a representative chess pawn double advance (e2 -> e4). Compares execution with
/// EnableStateHashing disabled vs enabled to provide an approximate per-event cost. This
/// isolates hashing cost; other feature flags remain at their defaults.
/// </remarks>
[MemoryDiagnoser]
public class HashingOverheadBenchmark
{
    private GameProgress _withHashing = null!;
    private GameProgress _withoutHashing = null!;
    private Piece _pawn = null!;
    private TilePath _path = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Baseline builder without hashing (toggle global flag)
        var original = Veggerby.Boards.Internal.FeatureFlags.EnableStateHashing;
        try
        {
            Veggerby.Boards.Internal.FeatureFlags.EnableStateHashing = false;
            _withoutHashing = new ChessGameBuilder().Compile();

            Veggerby.Boards.Internal.FeatureFlags.EnableStateHashing = true;
            _withHashing = new ChessGameBuilder().Compile();
        }
        finally
        {
            // restore to avoid affecting other benchmarks executed in same process
            Veggerby.Boards.Internal.FeatureFlags.EnableStateHashing = original;
        }

        var from = _withHashing.Game.GetTile("e2");
        var to = _withHashing.Game.GetTile("e4");
        _pawn = _withHashing.Game.GetPiece("white-pawn-2");
        _path = new ResolveTilePathPatternVisitor(_withHashing.Game.Board, from, to).ResultPath!;
    }

    [Benchmark(Baseline = true)]
    public GameProgress MovePawn_NoHashing()
    {
        return _withoutHashing.HandleEvent(new MovePieceGameEvent(_pawn, _path));
    }

    [Benchmark]
    public GameProgress MovePawn_WithHashing()
    {
        return _withHashing.HandleEvent(new MovePieceGameEvent(_pawn, _path));
    }
}