using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmark scaffold measuring impact of EventKind filtering fast-path on evaluation cost.
/// </summary>
/// <remarks>
/// Current implementation uses existing builders without additional tagging breadth. Once broader
/// EventKind coverage (e.g., capture, promotion, doubling) is introduced, expand scenarios:
///  - Mixed sequence (Move / Roll alternation) measuring skipped predicate evaluations.
///  - High-rule-density phase to quantify group-level pruning benefit.
/// The benchmark currently focuses on parity validation and provides a hook for future expansion.
/// </remarks>
[MemoryDiagnoser]
public class EventFilteringBaseline
{
    private GameProgress _chess = null!;
    private GameProgress _backgammon = null!;
    private TilePath _pawnPath = null!;
    private Piece _whitePawn = null!;
    private Dice _die1 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _chess = new ChessGameBuilder().Compile();
        _backgammon = new BackgammonGameBuilder().Compile();

        var from = _chess.Game.GetTile(E2);
        var to = _chess.Game.GetTile(E4);
        _whitePawn = _chess.Game.GetPiece(WhitePawn2);
        _pawnPath = new ResolveTilePathPatternVisitor(_chess.Game.Board, from, to).ResultPath!;
        _die1 = _backgammon.Game.GetArtifact<Dice>("dice-1");
    }

    [Benchmark(Description = "Handle Move (Chess)")]
    public GameProgress ChessMove()
    {
        return _chess.HandleEvent(new MovePieceGameEvent(_whitePawn, _pawnPath));
    }

    [Benchmark(Description = "Handle Roll (Backgammon)")]
    public GameProgress BackgammonRoll()
    {
        // Re-roll active dice to exercise Roll event path and filtering skip of Move-only rules.
        // Use a synthetic DiceState to exercise filtering path (value doesn't matter for benchmark parity)
        var synthetic = new DiceState<int>(_die1, 0);
        return _backgammon.HandleEvent(new RollDiceGameEvent<int>(synthetic));
    }
}