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
    private Piece _pawnWith = null!;
    private Piece _pawnWithout = null!;
    private TilePath _pathWith = null!;
    private TilePath _pathWithout = null!;

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

        // Resolve identical logical move for both compiled instances (flag on/off) separately to avoid null path usage
        var fromWith = _withSequencing.Game.GetTile(ChessIds.Tiles.E2) ?? throw new InvalidOperationException("TurnSequencing benchmark: from tile e2 missing (with sequencing)");
        var toWith = _withSequencing.Game.GetTile(ChessIds.Tiles.E4) ?? throw new InvalidOperationException("TurnSequencing benchmark: to tile e4 missing (with sequencing)");
        _pawnWith = _withSequencing.Game.GetPiece(ChessIds.Pieces.WhitePawn5) ?? throw new InvalidOperationException("TurnSequencing benchmark: pawn5 missing (with sequencing)"); // white pawn on e2 in builder setup
        var pathWithVisitor = new ResolveTilePathPatternVisitor(_withSequencing.Game.Board, fromWith, toWith);
        _pathWith = pathWithVisitor.ResultPath!;
        if (_pathWith is null)
        {
            // Fallback: construct simple two-step forward path (e2->e3->e4) if not resolved by patterns
            var mid = _withSequencing.Game.GetTile(ChessIds.Tiles.E3) ?? throw new InvalidOperationException("TurnSequencing benchmark: mid tile e3 missing (with sequencing)");
            var rel1 = _withSequencing.Game.Board.GetTileRelation(fromWith, mid);
            var rel2 = _withSequencing.Game.Board.GetTileRelation(mid, toWith);
            if (rel1 is not null && rel2 is not null)
            {
                var partial = TilePath.Create(null, rel1);
                _pathWith = TilePath.Create(partial, rel2);
            }
        }

        var fromWithout = _withoutSequencing.Game.GetTile(ChessIds.Tiles.E2) ?? throw new InvalidOperationException("TurnSequencing benchmark: from tile e2 missing (without sequencing)");
        var toWithout = _withoutSequencing.Game.GetTile(ChessIds.Tiles.E4) ?? throw new InvalidOperationException("TurnSequencing benchmark: to tile e4 missing (without sequencing)");
        _pawnWithout = _withoutSequencing.Game.GetPiece(ChessIds.Pieces.WhitePawn5) ?? throw new InvalidOperationException("TurnSequencing benchmark: pawn5 missing (without sequencing)");
        var pathWithoutVisitor = new ResolveTilePathPatternVisitor(_withoutSequencing.Game.Board, fromWithout, toWithout);
        _pathWithout = pathWithoutVisitor.ResultPath!;
        if (_pathWithout is null)
        {
            var mid2 = _withoutSequencing.Game.GetTile(ChessIds.Tiles.E3) ?? throw new InvalidOperationException("TurnSequencing benchmark: mid tile e3 missing (without sequencing)");
            var rel1b = _withoutSequencing.Game.Board.GetTileRelation(fromWithout, mid2);
            var rel2b = _withoutSequencing.Game.Board.GetTileRelation(mid2, toWithout);
            if (rel1b is not null && rel2b is not null)
            {
                var partial2 = TilePath.Create(null, rel1b);
                _pathWithout = TilePath.Create(partial2, rel2b);
            }
        }
    }

    [Benchmark(Baseline = true)]
    public GameProgress MovePawn_NoSequencing()
    {
        return _pathWithout is null ? _withoutSequencing : _withoutSequencing.HandleEvent(new MovePieceGameEvent(_pawnWithout, _pathWithout));
    }

    [Benchmark]
    public GameProgress MovePawn_WithSequencing()
    {
        return _pathWith is null ? _withSequencing : _withSequencing.HandleEvent(new MovePieceGameEvent(_pawnWith, _pathWith));
    }
}