using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.Constants;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.Internal.Paths;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Focused microbenchmark isolating sliding fast-path reconstruction cost under specific scenarios:
/// 1. Empty ray (no blockers)
/// 2. Blocked one step before destination
/// 3. Capture at destination (opponent piece)
/// 4. Off-ray destination (miss -> delegate)
/// Measures: compiled only vs fast-path (bitboards + sliding) variants.
/// </summary>
[MemoryDiagnoser]
public class SlidingFastPathMicroBenchmark
{
    private Game _game = null!;
    private BoardShape _shape = null!;
    private Piece _rook = null!;
    private Tile _a1 = null!; private Tile _a5 = null!; private Tile _a4 = null!; private Tile _a3 = null!; private Tile _b1 = null!;
    private GameProgress _progressEmpty = null!;
    private GameProgress _progressBlocked = null!;
    private GameProgress _progressCapture = null!;
    private GameProgress _progressOffRay = null!; // logically same as empty but target not on ray

    [GlobalSetup]
    public void Setup()
    {
        var north = new Direction(Constants.Directions.North);
        var south = new Direction(Constants.Directions.South);
        var east = new Direction(Constants.Directions.East);
        var west = new Direction(Constants.Directions.West);
        var boardTiles = new List<Tile>();
        for (int r = 1; r <= 8; r++)
        {
            boardTiles.Add(new Tile($"tile-a{r}"));
        }
        // Simple file (a1..a8) plus one lateral tile for off-ray target
        var lateral = new Tile(ChessIds.Tiles.B1);
        boardTiles.Add(lateral);
        var relations = new List<TileRelation>();
        for (int r = 1; r <= 8; r++)
        {
            if (r < 8)
            {
                relations.Add(new TileRelation(boardTiles[r - 1], boardTiles[r], north));
            }
            if (r > 1)
            {
                relations.Add(new TileRelation(boardTiles[r - 1], boardTiles[r - 2 + 1], south));
            }
        }
        // lateral east/west from a1 <-> b1
        relations.Add(new TileRelation(boardTiles[0], lateral, east));
        relations.Add(new TileRelation(lateral, boardTiles[0], west));
        var board = new Board("micro-sliding", relations);
        var white = new Player("white");
        var black = new Player("black");
        _rook = new Piece("rook", white, new[] { new DirectionPattern(north, true) });
        var artifacts = new List<Artifact> { board, white, black, _rook };
        _game = new Game(board, new[] { white, black }, artifacts);
        _shape = BoardShape.Build(board);

        _a1 = boardTiles[0];
        _a5 = boardTiles[4];
        _a4 = boardTiles[3];
        _a3 = boardTiles[2];
        _b1 = lateral;

        _progressEmpty = BuildProgress(new List<(Piece piece, Tile tile)> { (_rook, _a1) });
        // Blocker at a4 (one before destination a5)
        _progressBlocked = BuildProgress(new List<(Piece piece, Tile tile)> { (_rook, _a1), (new Piece("blocker-a4", black, new[] { new NullPattern() }), _a4) });
        // Capture piece at a5
        _progressCapture = BuildProgress(new List<(Piece piece, Tile tile)> { (_rook, _a1), (new Piece("victim-a5", black, new[] { new NullPattern() }), _a5) });
        _progressOffRay = _progressEmpty; // target will be lateral b1
    }

    [Params("empty", "blocked", "capture", "offray")] public string Scenario { get; set; } = "empty";

    [Benchmark(Baseline = true)]
    public bool CompiledOnly()
    {
        var progress = SelectProgress();
        var target = SelectTarget();
        var path = progress.ResolvePathCompiledFirst(_rook, _a1, target);
        return path is not null && path.To.Equals(target);
    }

    [Benchmark]
    public bool FastPath()
    {
        var progress = SelectProgress();
        var target = SelectTarget();
        var path = progress.ResolvePathCompiledFirst(_rook, _a1, target);
        return path is not null && path.To.Equals(target);
    }

    [Benchmark(Description = "FastPathAllocationProbe (empty only)")]
    public bool FastPathAllocationProbeEmpty()
    {
        if (Scenario != "empty")
        {
            return false; // only measured for empty-ray invariant scenario
        }
        var progress = _progressEmpty;
        var target = _a5;
        var probe = FastPathAllocationProbe.Start();
        bool result = false;
        for (int i = 0; i < 5000; i++)
        {
            var path = progress.ResolvePathCompiledFirst(_rook, _a1, target);
            if (path is not null && path.To.Equals(target))
            {
                result = true;
            }
        }
        var snapshot = probe.Stop();
        // soft assertion: we expect zero collections; returning result so benchmark baseline metrics remain comparable
        if (!snapshot.IsZeroCollections)
        {
            // Do not throw (would invalidate benchmark); simply return false to highlight deviation in summary if surfaced.
            return false;
        }
        return result;
    }

    private GameProgress SelectProgress() => Scenario switch
    {
        "blocked" => _progressBlocked,
        "capture" => _progressCapture,
        _ => _progressEmpty
    };

    private Tile SelectTarget() => Scenario switch
    {
        "blocked" => _a5, // expectation: false
        "capture" => _a5, // expectation: true
        "offray" => _b1, // expectation: false
        _ => _a5 // empty expectation: true
    };

    private GameProgress BuildProgress(IEnumerable<(Piece piece, Tile tile)> placements)
    {
        var artifactStates = new List<IArtifactState>();
        foreach (var (piece, tile) in placements)
        {
            artifactStates.Add(new PieceState(piece, tile));
        }
        var state = GameState.New(artifactStates);
        var topology = new Internal.Topology.BoardShapeTopologyAdapter(_shape);

        // Path resolver selection (compiled only path used inside GameProgress; sliding fast-path decoration applied later if enabled)
        IPathResolver resolver;
        var table = PatternCompiler.Compile(_game);
        var compiled = new CompiledPatternResolver(table, _game.Board, null, _shape);
        resolver = new CompiledPathResolverAdapter(compiled);

        // Acceleration context
        Internal.Acceleration.IAccelerationContext accelerationContext;
        // Always use bitboards (graduated feature)
        var pieceLayout = PieceMapLayout.Build(_game);
        var pieceSnapshot = PieceMapSnapshot.Build(pieceLayout, state, _shape);
        var bbLayout = BitboardLayout.Build(_game);
        var bbSnapshot = BitboardSnapshot.Build(bbLayout, state, _shape);
        var occupancy = new Internal.Occupancy.BitboardOccupancyIndex(bbLayout, bbSnapshot, _shape, _game, state);
        var sliding = Internal.Attacks.SlidingAttackGenerator.Build(_shape);
        accelerationContext = new Internal.Acceleration.BitboardAccelerationContext(bbLayout, bbSnapshot, pieceLayout, pieceSnapshot, _shape, topology, occupancy, sliding);
        // Always use sliding fast path (graduated feature)
        resolver = new SlidingFastPathResolver(_shape, sliding, accelerationContext.Occupancy, resolver);

        var capabilities = new EngineCapabilities(topology, resolver, accelerationContext);
        var phaseRoot = GamePhase.New(1, string.Empty, new States.Conditions.NullGameStateCondition(), Flows.Rules.GameEventRule<Flows.Events.IGameEvent>.Null);
        var plan = DecisionPlan.Compile(phaseRoot);
        var engine = new GameEngine(_game, phaseRoot, plan, Flows.Observers.NullEvaluationObserver.Instance, capabilities);
        // Benchmarks start with empty immutable event chain.
        return new GameProgress(engine, state, Veggerby.Boards.Flows.Events.EventChain.Empty);
    }
}