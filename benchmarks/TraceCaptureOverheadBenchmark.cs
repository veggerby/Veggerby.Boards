using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures relative overhead of enabling trace capture (feature flag EnableTraceCapture) during HandleEvent.
/// </summary>
[MemoryDiagnoser]
public class TraceCaptureOverheadBenchmark
{
    [Params(false, true)]
    public bool EnableTraceCapture { get; set; }

    private GameProgress _progress = null!;
    private MovePieceGameEvent _event = null!;

    [GlobalSetup]
    public void Setup()
    {
        var original = FeatureFlags.EnableTraceCapture;
        try
        {
            FeatureFlags.EnableTraceCapture = EnableTraceCapture;
            var builder = new ChessGameBuilder();
            _progress = builder.Compile();
            var game = _progress.Game;
            var pawn = game.GetPiece("white-pawn-2");
            var from = game.GetTile("e2");
            var to = game.GetTile("e4");
            var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            pawn.Patterns.First().Accept(visitor);
            var path = visitor.ResultPath!;
            _event = new MovePieceGameEvent(pawn, path);
        }
        finally
        {
            FeatureFlags.EnableTraceCapture = original; // restore static for other benchmarks
        }
    }

    [Benchmark]
    public GameProgress HandleEvent()
    {
        return _progress.HandleEvent(_event);
    }
}