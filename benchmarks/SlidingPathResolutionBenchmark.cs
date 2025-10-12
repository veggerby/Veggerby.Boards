using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks sliding path resolution (rook-style and bishop-style rays) under varying occupancies
/// comparing: legacy visitor, compiled resolver only, and compiled+bitboards fast-path.
/// </summary>
[MemoryDiagnoser]
public class SlidingPathResolutionBenchmark
{
    private Game _game = null!;
    private CompiledPatternResolver _compiled = null!;
    private BoardShape _shape = null!; // internal topology primitive (still used locally for building acceleration)
    private List<(Piece piece, Tile from, Tile to)> _queriesEmpty = null!;
    private List<(Piece piece, Tile from, Tile to)> _queriesQuarter = null!;
    private List<(Piece piece, Tile from, Tile to)> _queriesHalf = null!;
    private GameProgress _progressEmpty = null!; // for fast-path benchmark (bitboards + piece map)
    private GameProgress _progressQuarter = null!;
    private GameProgress _progressHalf = null!;
    private Piece _rook = null!;
    private Piece _bishop = null!;
    private Piece _queen = null!;
    private Tile _d4 = null!;
    private Tile _h4 = null!;
    private Tile _d8 = null!;
    private Tile _c1 = null!;
    private Tile _g5 = null!;

    [Params("empty", "quarter", "half")] public string Density { get; set; } = "empty";

    [GlobalSetup]
    public void Setup()
    {
        // Build full 8x8 grid (tile-a1..tile-h8) with orthogonal + diagonal directions
        var north = new Direction(Constants.Directions.North);
        var south = new Direction(Constants.Directions.South);
        var east = new Direction(Constants.Directions.East);
        var west = new Direction(Constants.Directions.West);
        var ne = new Direction(Constants.Directions.NorthEast);
        var nw = new Direction(Constants.Directions.NorthWest);
        var se = new Direction(Constants.Directions.SouthEast);
        var sw = new Direction(Constants.Directions.SouthWest);
        var dirs = new[] { north, south, east, west, ne, nw, se, sw };
        var tiles = new List<Tile>();
        var relations = new List<TileRelation>();

        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];

        for (int r = 1; r <= 8; r++)
        {
            foreach (var f in files)
            {
                tiles.Add(new Tile($"tile-{f}{r}"));
            }
        }

        Tile T(char f, int r) => tiles.Single(t => t.Id == $"tile-{f}{r}");
        foreach (var f in files)
        {
            for (int r = 1; r <= 8; r++)
            {
                if (r < 8)
                {
                    relations.Add(new TileRelation(T(f, r), T(f, r + 1), north));
                }

                if (r > 1)
                {
                    relations.Add(new TileRelation(T(f, r), T(f, r - 1), south));
                }

                if (f < 'h')
                {
                    relations.Add(new TileRelation(T(f, r), T((char)(f + 1), r), east));
                }

                if (f > 'a')
                {
                    relations.Add(new TileRelation(T(f, r), T((char)(f - 1), r), west));
                }

                if (f < 'h' && r < 8)
                {
                    relations.Add(new TileRelation(T(f, r), T((char)(f + 1), r + 1), ne));
                }

                if (f > 'a' && r < 8)
                {
                    relations.Add(new TileRelation(T(f, r), T((char)(f - 1), r + 1), nw));
                }

                if (f < 'h' && r > 1)
                {
                    relations.Add(new TileRelation(T(f, r), T((char)(f + 1), r - 1), se));
                }

                if (f > 'a' && r > 1)
                {
                    relations.Add(new TileRelation(T(f, r), T((char)(f - 1), r - 1), sw));
                }
            }
        }

        var board = new Board("bench-sliding-8x8", relations);
        var white = new Player("white"); var black = new Player("black");

        // Slider archetypes
        _rook = new Piece("rook", white, [new DirectionPattern(north, true), new DirectionPattern(east, true), new DirectionPattern(south, true), new DirectionPattern(west, true)]);
        _bishop = new Piece("bishop", white, [new DirectionPattern(ne, true), new DirectionPattern(nw, true), new DirectionPattern(se, true), new DirectionPattern(sw, true)]);
        _queen = new Piece("queen", white, [new MultiDirectionPattern(dirs, true)]);

        // Populate all pieces (movers + blockers) into artifact list
        var artifacts = new List<Artifact> { _rook, _bishop, _queen, white, black, board };
        _game = new Game(board, [white, black], artifacts);

        var table = PatternCompiler.Compile(_game);
        _shape = BoardShape.Build(board);
        _compiled = new CompiledPatternResolver(table, board, null, _shape);

        // Queries: representative rays (orthogonal & diagonal) various lengths
        _d4 = T('d', 4);
        _h4 = T('h', 4);
        _d8 = T('d', 8);
        _c1 = T('c', 1);
        _g5 = T('g', 5);
        Tile a4 = T('a', 4); // unused but retained for potential future targets
        Tile d1 = T('d', 1); // unused but retained

        _queriesEmpty = [(_rook, _d4, _h4), (_rook, _d4, _d8), (_bishop, _c1, _g5), (_queen, _d4, _h4), (_queen, _d4, _d8)];

        // Densities: choose a repeatable pseudo random subset for blockers (25% and 50%)
        var rnd = new System.Random(1234);
        var candidateSquares = tiles.Where(t => t != _d4 && t != _h4 && t != _d8 && t != _c1 && t != _g5).ToList();
        var quarterCount = candidateSquares.Count / 4;
        var halfCount = candidateSquares.Count / 2;
        var quarter = candidateSquares.OrderBy(_ => rnd.Next()).Take(quarterCount).ToHashSet();
        var half = candidateSquares.OrderBy(_ => rnd.Next()).Take(halfCount).ToHashSet();

        _queriesQuarter = _queriesEmpty; // same target set; occupancy influences pruning only
        _queriesHalf = _queriesEmpty;

        // Build initial progress instances for each density scenario (bitboards + piece map enabled via feature flags).
        BuildProgressScenarios(tiles, quarter, half, white);
    }

    [IterationSetup]
    public static void IterationSetup()
    {
        // No per-iteration rebuild required; scenarios pre-built in GlobalSetup to isolate path resolution cost.
        // If future mutations are introduced, rebuild here to avoid skew from incremental updates being benchmarked.
    }

    [Benchmark(Baseline = true)]
    public int LegacyVisitor()
    {
        int count = 0;
        foreach (var (piece, from, to) in SelectQueries())
        {
            foreach (var pattern in piece.Patterns)
            {
                var visitor = new ResolveTilePathPatternVisitor(_game.Board, from, to);
                pattern.Accept(visitor);
                if (visitor.ResultPath is not null && visitor.ResultPath.To.Equals(to))
                {
                    count++; break;
                }
            }
        }

        return count;
    }

    [Benchmark]
    public int Compiled()
    {
        int count = 0;
        foreach (var (piece, from, to) in SelectQueries())
        {
            if (_compiled.TryResolve(piece, from, to, out var path) && path is not null)
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int FastPath()
    {
        // Enable full fast-path stack.
        FeatureFlags.EnableBitboards = true;
        FeatureFlags.EnableCompiledPatterns = true;
        FeatureFlags.EnableSlidingFastPath = true;
        var progress = SelectProgress();
        return ResolveSet(progress);
    }

    [Benchmark]
    public int CompiledWithBitboardsNoFastPath()
    {
        // Bitboards + compiled patterns enabled, but sliding fast-path disabled (measures overhead neutrality).
        FeatureFlags.EnableBitboards = true;
        FeatureFlags.EnableCompiledPatterns = true;
        FeatureFlags.EnableSlidingFastPath = false;
        var progress = SelectProgress();
        return ResolveSet(progress);
    }

    [Benchmark]
    public int CompiledNoBitboards()
    {
        // Pure compiled resolver path (no bitboards).
        FeatureFlags.EnableBitboards = false;
        FeatureFlags.EnableCompiledPatterns = true;
        FeatureFlags.EnableSlidingFastPath = false;
        // Use empty progress (bitboards not required because we won't exercise fast-path); queries derived from shared game state.
        return ResolveCompiledOnly();
    }

    private GameProgress SelectProgress()
    {
        return Density switch
        {
            "quarter" => _progressQuarter,
            "half" => _progressHalf,
            _ => _progressEmpty
        };
    }

    private int ResolveSet(GameProgress progress)
    {
        int count = 0;
        foreach (var (piece, from, to) in SelectQueries())
        {
            var path = progress.ResolvePathCompiledFirst(piece, from, to);
            if (path is not null && path.To.Equals(to))
            {
                count++;
            }
        }
        return count;
    }

    private int ResolveCompiledOnly()
    {
        int count = 0;
        foreach (var (piece, from, to) in SelectQueries())
        {
            if (_compiled.TryResolve(piece, from, to, out var path) && path is not null && path.To.Equals(to))
            {
                count++;
            }
        }
        return count;
    }

    private IEnumerable<(Piece piece, Tile from, Tile to)> SelectQueries()
    {
        return Density switch
        {
            "empty" => _queriesEmpty,
            "quarter" => _queriesQuarter,
            "half" => _queriesHalf,
            _ => _queriesEmpty
        };
    }

    private void BuildProgressScenarios(IEnumerable<Tile> tiles, HashSet<Tile> quarter, HashSet<Tile> half, Player owner)
    {
        // Build artificial GameProgress instances with different blocker densities by constructing GameState snapshots.
        // Place sliding pieces on their source squares; add blockers as anonymous pieces owned by opponent for capture / blocking semantics.
        var opponent = _game.Players.First(p => p != owner);
        GameProgress Build(HashSet<Tile> blockers)
        {
            var artifactStates = new List<IArtifactState>();
            // Primary sliders (placed deterministically)
            artifactStates.Add(new PieceState(_rook, _d4));
            artifactStates.Add(new PieceState(_bishop, _c1));
            artifactStates.Add(new PieceState(_queen, _d4)); // queen shares rook origin intentionally
            // Blockers – create temporary artifacts (pieces) each run; identity uniqueness not required for benchmark semantics.
            int i = 0;
            foreach (var t in blockers)
            {
                var blocker = new Piece($"blocker-{i++}-{t.Id}", opponent, new[] { new NullPattern() });
                // Register artifact in game (unsafe to mutate artifacts post construction ordinarily; here we rely on local reference list for state only).
                artifactStates.Add(new PieceState(blocker, t));
            }
            var state = GameState.New(artifactStates);

            // Build minimal services replicating builder compile path for acceleration features.
            // Build minimal capability triplet (Topology + PathResolver + AccelerationContext) matching production GameBuilder logic.
            var topology = new Internal.Topology.BoardShapeTopologyAdapter(_shape);

            // Path resolver (compiled patterns always enabled for benchmark scenarios to measure compiled vs fast-path cost).
            Internal.Paths.IPathResolver pathResolver;
            if (FeatureFlags.EnableCompiledPatterns)
            {
                var table = PatternCompiler.Compile(_game);
                var resolver = new CompiledPatternResolver(table, _game.Board, null, _shape);
                pathResolver = new Internal.Paths.CompiledPathResolverAdapter(resolver);
            }
            else
            {
                pathResolver = new Internal.Paths.SimplePatternPathResolver(_game.Board);
            }

            // Acceleration context selection (mimic builder but only for occupancy + attacks; piece map + bitboards internal).
            Internal.Acceleration.IAccelerationContext accelerationContext;
            if (FeatureFlags.EnableBitboards && _game.Board.Tiles.Count() <= 64)
            {
                var pieceLayout = PieceMapLayout.Build(_game);
                var pieceSnapshot = PieceMapSnapshot.Build(pieceLayout, state, _shape);
                var bbLayout = BitboardLayout.Build(_game);
                var bbSnapshot = BitboardSnapshot.Build(bbLayout, state, _shape);
                var occupancy = new Internal.Occupancy.BitboardOccupancyIndex(bbLayout, bbSnapshot, _shape, _game, state);
                var sliding = Internal.Attacks.SlidingAttackGenerator.Build(_shape);
                accelerationContext = new Internal.Acceleration.BitboardAccelerationContext(bbLayout, bbSnapshot, pieceLayout, pieceSnapshot, _shape, topology, occupancy, sliding);
            }
            else
            {
                var occupancy = new Internal.Occupancy.NaiveOccupancyIndex(_game, state);
                var sliding = Internal.Attacks.SlidingAttackGenerator.Build(_shape);
                accelerationContext = new Internal.Acceleration.NaiveAccelerationContext(occupancy, sliding);
            }

            // Optional sliding fast-path decoration respecting feature flag.
            if (FeatureFlags.EnableSlidingFastPath)
            {
                var sliding = Internal.Attacks.SlidingAttackGenerator.Build(_shape);
                pathResolver = new Internal.Paths.SlidingFastPathResolver(_shape, sliding, accelerationContext.Occupancy, pathResolver);
            }

            var capabilities = new EngineCapabilities(topology, pathResolver, accelerationContext);

            // Minimal phase root (no rules needed for path resolution benchmark)
            var phaseRoot = GamePhase.New(1, "n/a", new States.Conditions.NullGameStateCondition(), Flows.Rules.GameEventRule<Flows.Events.IGameEvent>.Null);
            // DecisionPlan mandatory (GameEngine enforces non-null); compile minimal plan for benchmark phase root.
            var plan = DecisionPlan.Compile(phaseRoot);
            var engine = new GameEngine(_game, phaseRoot, plan, Flows.Observers.NullEvaluationObserver.Instance, capabilities);
            return new GameProgress(engine, state, null);
        }

        _progressEmpty = Build(new HashSet<Tile>());
        _progressQuarter = Build(quarter);
        _progressHalf = Build(half);
    }
}