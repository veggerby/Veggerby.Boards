using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures relative overhead of enabling turn sequencing (feature flag <c>EnableTurnSequencing</c>)
/// during a representative single move event (white pawn e2→e4). Target: &lt;3% p50 latency delta.
/// </summary>
/// <remarks>
/// Benchmark strategy mirrors <see cref="HashingOverheadBenchmark"/>: two compiled game instances are
/// prepared under distinct feature flag configurations (ON/OFF). The per-iteration benchmark simply
/// applies a deterministic double pawn advance. All other feature flags remain at their default values.
/// Style charter adhered to: file-scoped namespace, explicit braces, no LINQ in benchmark hot path,
/// immutable state transitions. This benchmark does not assert the threshold; consumers should compare
/// results manually or integrate with an external regression gate.
/// </remarks>
[MemoryDiagnoser]
public class TurnSequencingOverheadBenchmark
{
    private GameProgress _withSequencing = null!;
    private GameProgress _withoutSequencing = null!;
    private Piece _pawn = null!;
    private TilePath _path = null!;

    [GlobalSetup]
    public void Setup()
    {
        var original = Internal.FeatureFlags.EnableTurnSequencing;
        try
        {
            Internal.FeatureFlags.EnableTurnSequencing = false;
            _withoutSequencing = new ChessGameBuilder().Compile();

            Internal.FeatureFlags.EnableTurnSequencing = true;
            _withSequencing = new ChessGameBuilder().Compile();
        }
        finally
        {
            Internal.FeatureFlags.EnableTurnSequencing = original;
        }

        var from = _withSequencing.Game.GetTile("e2");
        var to = _withSequencing.Game.GetTile("e4");
        _pawn = _withSequencing.Game.GetPiece("white-pawn-2");
        _path = new ResolveTilePathPatternVisitor(_withSequencing.Game.Board, from, to).ResultPath!;
    }

    [Benchmark(Baseline = true)]
    public GameProgress MovePawn_NoSequencing()
    {
        return _withoutSequencing.HandleEvent(new MovePieceGameEvent(_pawn, _path));
    }

    [Benchmark]
    public GameProgress MovePawn_WithSequencing()
    {
        return _withSequencing.HandleEvent(new MovePieceGameEvent(_pawn, _path));
    }
}