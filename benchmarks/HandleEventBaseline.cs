using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Baseline benchmark harness capturing current (pre-DecisionPlan) performance characteristics.
/// </summary>
/// <remarks>
/// This benchmark establishes a stable performance reference for a representative chess move event
/// (white pawn double advance) prior to the introduction of the DecisionPlan executor and other
/// planned optimizations. Additional scenarios (e.g., Backgammon opening moves, complex path
/// resolution, capture sequences) should be appended in future PRs to broaden coverage.
/// </remarks>
[MemoryDiagnoser]
public class HandleEventBaseline
{
    private GameProgress _progress = null!;
    private Piece _whitePawn = null!;
    private Tile _from = null!;
    private Tile _to = null!;
    private TilePath _path = null!;

    /// <summary>
    /// Global benchmark setup creating a compiled Chess game and resolving the reference move path.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var builder = new ChessGameBuilder();
        _progress = builder.Compile();
        _whitePawn = _progress.Game.GetPiece("white-pawn-2");
        _from = _progress.Game.GetTile("e2");
        _to = _progress.Game.GetTile("e4");
        _path = new ResolveTilePathPatternVisitor(_progress.Game.Board, _from, _to).ResultPath!;
    }

    /// <summary>
    /// Benchmarks handling a standard opening double-step pawn move (e2 -> e4).
    /// </summary>
    /// <returns>The resulting <see cref="GameProgress"/> after applying the move event.</returns>
    [Benchmark]
    public GameProgress MovePawnTwoSquares()
    {
        return _progress.HandleEvent(new MovePieceGameEvent(_whitePawn, _path));
    }
}