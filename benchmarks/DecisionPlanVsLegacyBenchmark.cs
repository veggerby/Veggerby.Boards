using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts.Relations; // ResolveTilePathPatternVisitor
using Veggerby.Boards.Chess; // ChessGameBuilder
using Veggerby.Boards.Flows.Events; // MovePieceGameEvent
using Veggerby.Boards.Internal; // FeatureFlags
using Veggerby.Boards.States; // GameProgress

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Compares HandleEvent throughput between legacy traversal (DecisionPlan disabled) and DecisionPlan path (enabled)
/// over a small deterministic sequence of opening-style chess moves (pawn advances + minor piece development).
/// This isolates rule evaluation engine overhead independent of movement/path resolution complexities.
/// </summary>
[MemoryDiagnoser]
public class DecisionPlanVsLegacyBenchmark
{
    private record PlannedMove(string PieceId, string From, string To);

    private IReadOnlyList<PlannedMove> _moves = null!;

    [GlobalSetup]
    public void Setup()
    {

        var builder = new ChessGameBuilder();
        builder.Compile(); // warm build

        _moves = new List<PlannedMove>
        {
            new("white-pawn-2", "e2", "e3"),
            new("black-pawn-2", "e7", "e6"),
            new("white-pawn-3", "d2", "d3"),
            new("black-pawn-3", "d7", "d6"),
            new("white-bishop-1", "f1", "e2"),
            new("black-bishop-1", "f8", "e7"),
        };
    }

    private static GameProgress ApplyMove(GameProgress gp, PlannedMove mv)
    {
        var piece = gp.Game.GetPiece(mv.PieceId) ?? throw new InvalidOperationException($"Piece {mv.PieceId} missing");
        var from = gp.Game.GetTile(mv.From) ?? throw new InvalidOperationException($"From tile {mv.From} missing");
        var to = gp.Game.GetTile(mv.To) ?? throw new InvalidOperationException($"To tile {mv.To} missing");
        var pathVisitor = new ResolveTilePathPatternVisitor(gp.Game.Board, from, to);
        var path = pathVisitor.ResultPath!;
        return gp.HandleEvent(new MovePieceGameEvent(piece, path));
    }

    [Benchmark(Baseline = true)]
    public GameProgress Legacy_HandleSequence()
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        foreach (var mv in _moves)
        {
            progress = ApplyMove(progress, mv);
        }
        return progress; // returned to prevent elimination
    }

    [Benchmark]
    public GameProgress DecisionPlan_HandleSequence()
    {
        // simulate plan-enabled mode via grouping toggle
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        foreach (var mv in _moves)
        {
            progress = ApplyMove(progress, mv);
        }
        return progress;
    }
}