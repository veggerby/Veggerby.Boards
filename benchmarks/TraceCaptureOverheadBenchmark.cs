using BenchmarkDotNet.Attributes;

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
            var pawn = game.GetPiece(ChessIds.Pieces.WhitePawn2);
            var from = game.GetTile(ChessIds.Tiles.E2);
            var to = game.GetTile(ChessIds.Tiles.E4);
            if (pawn is null || from is null || to is null)
            {
                // Fallback: leave _event as null!; benchmark invocation will throw early which is acceptable for setup failure.
                throw new InvalidOperationException("Required chess artifacts not found for benchmark setup.");
            }
            var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            pawn.Patterns.First().Accept(visitor);
            var path = visitor.ResultPath ?? throw new InvalidOperationException("Expected non-null path for benchmark pawn pattern.");
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