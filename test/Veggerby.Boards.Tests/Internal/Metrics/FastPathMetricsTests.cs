using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;
using Veggerby.Boards.Tests.Utils;

using Xunit;

namespace Veggerby.Boards.Tests.Internal.Metrics;

public class FastPathMetricsTests
{
    private static GameProgress BuildProgress(bool bitboards)
    {
        // Build minimal 4x4 board with orthogonal + diagonal dirs
        var north = new Direction("north");
        var east = new Direction("east");
        var south = new Direction("south");
        var west = new Direction("west");
        var ne = new Direction("north-east");
        var nw = new Direction("north-west");
        var se = new Direction("south-east");
        var sw = new Direction("south-west");

        var tiles = new List<Tile>();
        for (int r = 1; r <= 4; r++)
        {
            for (int c = 1; c <= 4; c++) { tiles.Add(new Tile($"t{c}-{r}")); }
        }

        Tile T(int c, int r) => tiles.Single(t => t.Id == $"t{c}-{r}");
        var rels = new List<TileRelation>();
        for (int r = 1; r <= 4; r++)
        {
            for (int c = 1; c <= 4; c++)
            {
                if (r < 4) { rels.Add(new TileRelation(T(c, r), T(c, r + 1), north)); }
                if (r > 1) { rels.Add(new TileRelation(T(c, r), T(c, r - 1), south)); }
                if (c < 4) { rels.Add(new TileRelation(T(c, r), T(c + 1, r), east)); }
                if (c > 1) { rels.Add(new TileRelation(T(c, r), T(c - 1, r), west)); }
                if (c < 4 && r < 4) { rels.Add(new TileRelation(T(c, r), T(c + 1, r + 1), ne)); }
                if (c > 1 && r < 4) { rels.Add(new TileRelation(T(c, r), T(c - 1, r + 1), nw)); }
                if (c < 4 && r > 1) { rels.Add(new TileRelation(T(c, r), T(c + 1, r - 1), se)); }
                if (c > 1 && r > 1) { rels.Add(new TileRelation(T(c, r), T(c - 1, r - 1), sw)); }
            }
        }

        var board = new Board("fast-metrics-board", rels);
        var white = new Player("white");
        var rook = new Piece("rook", white, [new DirectionPattern(north, true)]);
        var game = new Game(board, [white], [rook]);
        var initial = GameState.New([new PieceState(rook, T(2, 2))]);

        var capabilities = new EngineCapabilities { Shape = BoardShape.Build(board) };

        if (bitboards)
        {
            var shape = capabilities.Shape;
            var pmLayout = PieceMapLayout.Build(game);
            var pmSnapshot = PieceMapSnapshot.Build(pmLayout, initial, shape);
            capabilities.PieceMap = new PieceMapServices(pmLayout, pmSnapshot);
            var bbLayout = BitboardLayout.Build(game);
            var bbSnapshot = BitboardSnapshot.Build(bbLayout, initial, shape);
            capabilities.Bitboards = new BitboardServices(bbLayout, bbSnapshot);
            var sliding = SlidingAttackGenerator.Build(shape);
            capabilities.Attacks = new AttackGeneratorServices(sliding);
        }

        var phase = GamePhase.New(1, "noop", new NullGameStateCondition(), new NoOpRule());
        var engine = new GameEngine(game, phase, null, NullEvaluationObserver.Instance, capabilities);
        PieceMapSnapshot pmSnap = capabilities.PieceMap?.Snapshot;
        BitboardSnapshot bbSnap = capabilities.Bitboards?.Snapshot;

        return new GameProgress(engine, initial, null, pmSnap, bbSnap);
    }

    [Fact]
    public void GivenBoardWithMoreThan64Tiles_WhenResolvingSlidingPath_ThenFastPathSkippedNoServicesIncrementsAndNoCrash()
    {
        using var flags = new FeatureFlagScope(bitboards: true, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();

        var north = new Direction("north");
        var tiles = new List<Tile>();
        for (int r = 1; r <= 8; r++)
        {
            for (int c = 1; c <= 9; c++) { tiles.Add(new Tile($"t{c}-{r}")); }
        }
        Tile T(int c, int r) => tiles.Single(t => t.Id == $"t{c}-{r}");
        var rels = new List<TileRelation>();
        for (int r = 1; r <= 7; r++)
        {
            for (int c = 1; c <= 9; c++) { rels.Add(new TileRelation(T(c, r), T(c, r + 1), north)); }
        }
        var board = new Board("gt64-board", rels);
        var white = new Player("white");
        var rook = new Piece("rook", white, [new DirectionPattern(north, true)]);
        var game = new Game(board, [white], [rook]);
        var initial = GameState.New([new PieceState(rook, T(1, 1))]);
        var capabilities = new EngineCapabilities { Shape = BoardShape.Build(board) };
        var phase = GamePhase.New(1, "noop", new NullGameStateCondition(), new NoOpRule());
        var engine = new GameEngine(game, phase, null, NullEvaluationObserver.Instance, capabilities);
        var progress = new GameProgress(engine, initial, null, null, null);
        var from = T(1, 1); var to = T(1, 8);
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        path.Should().NotBeNull();
        var snap = FastPathMetrics.Snapshot();
        snap.Attempts.Should().Be(1);
        snap.FastPathHits.Should().Be(0);
        snap.FastPathSkipNoServices.Should().BeGreaterThan(0);
        (snap.CompiledHits + snap.LegacyHits).Should().Be(1);
    }

    [Fact]
    public void GivenBitboardsEnabled_WhenResolvingSlidingPath_ThenFastPathHitIncrementsCounter()
    {
        using var flags = new FeatureFlagScope(bitboards: true, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();
        var progress = BuildProgress(bitboards: true);
        var rook = progress.Game.GetPiece("rook");
        var from = progress.State.GetStates<PieceState>().Single(ps => ps.Artifact == rook).CurrentTile;
        var to = progress.Game.GetTile("t2-4");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        path.Should().NotBeNull();
        var snap = FastPathMetrics.Snapshot();
        snap.Attempts.Should().Be(1);
        snap.FastPathHits.Should().Be(1);
        snap.CompiledHits.Should().Be(0);
        snap.FastPathSkipNotSlider.Should().Be(0);
        snap.FastPathSkipNoServices.Should().Be(0);
        snap.FastPathSkipAttackMiss.Should().Be(0);
        snap.FastPathSkipReconstructFail.Should().Be(0);
    }

    [Fact]
    public void GivenBitboardsDisabled_WhenResolvingSlidingPath_ThenFastPathSkippedNoPrereqIncrementsCounter()
    {
        using var flags = new FeatureFlagScope(bitboards: false, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();
        var progress = BuildProgress(bitboards: false);
        var rook = progress.Game.GetPiece("rook");
        var from = progress.State.GetStates<PieceState>().Single(ps => ps.Artifact == rook).CurrentTile;
        var to = progress.Game.GetTile("t2-4");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        path.Should().NotBeNull();
        var snap = FastPathMetrics.Snapshot();
        snap.Attempts.Should().Be(1);
        snap.FastPathHits.Should().Be(0);
        snap.FastPathSkippedNoPrereq.Should().Be(1);
        snap.FastPathSkipNoServices.Should().BeGreaterThan(0);
        (snap.CompiledHits + snap.LegacyHits).Should().Be(1);
    }

    [Fact]
    public void GivenNonSlider_WhenResolvingPath_ThenFastPathSkipNotSliderIncrements()
    {
        using var flags = new FeatureFlagScope(bitboards: true, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();
        var north = new Direction("north");
        var east = new Direction("east");
        var a = new Tile("a"); var b = new Tile("b");
        var board = new Board("ns-board", new[] { new TileRelation(a, b, east) });
        var white = new Player("white");
        var knight = new Piece("knight", white, new IPattern[] { new FixedPattern(new[] { east }) });
        var game = new Game(board, new[] { white }, new Artifact[] { knight });
        var state = GameState.New(new IArtifactState[] { new PieceState(knight, a) });
        var shape = BoardShape.Build(board);
        var capabilities2 = new EngineCapabilities { Shape = shape };
        var pmLayout = PieceMapLayout.Build(game);
        var pmSnapshot = PieceMapSnapshot.Build(pmLayout, state, shape);
        capabilities2.PieceMap = new PieceMapServices(pmLayout, pmSnapshot);
        var bbLayout = BitboardLayout.Build(game);
        var bbSnapshot = BitboardSnapshot.Build(bbLayout, state, shape);
        capabilities2.Bitboards = new BitboardServices(bbLayout, bbSnapshot);
        var sliding = SlidingAttackGenerator.Build(shape);
        capabilities2.Attacks = new AttackGeneratorServices(sliding);
        var phase = GamePhase.New(1, "noop", new NullGameStateCondition(), new NoOpRule());
        var engine = new GameEngine(game, phase, null, NullEvaluationObserver.Instance, capabilities2);
        var progress = new GameProgress(engine, state, null, pmSnapshot, bbSnapshot);
        var path = progress.ResolvePathCompiledFirst(knight, a, b);
        var snap = FastPathMetrics.Snapshot();
        snap.FastPathSkipNotSlider.Should().BeGreaterThan(0);
    }

    // NOTE: skip-attack-miss metric requires a mismatch between attack ray inclusion and direction reconstruction,
    // which is not currently achievable with consistent BoardShape adjacency; omitted from explicit test.

    private sealed class RookNorthBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "rook-north";
            AddDirection("north");
            AddPlayer("white");
            AddTile("v1"); AddTile("v2"); AddTile("v3"); AddTile("v4");
            WithTile("v1").WithRelationTo("v2").InDirection("north");
            WithTile("v2").WithRelationTo("v3").InDirection("north");
            WithTile("v3").WithRelationTo("v4").InDirection("north");
            AddPiece("rook").WithOwner("white").HasDirection("north").CanRepeat().OnTile("v1");
        }
    }

    [Fact]
    public void GivenCompiledPatternsEnabledAndFastPathPrereqsMissing_WhenResolving_ThenCompiledHitIncrements()
    {
        using var flags = new FeatureFlagScope(bitboards: false, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();
        var builder = new RookNorthBuilder();
        var progress = builder.Compile();
        var rook = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("v1");
        var to = progress.Game.GetTile("v4");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        path.Should().NotBeNull();
        var snap = FastPathMetrics.Snapshot();
        snap.Attempts.Should().Be(1);
        snap.CompiledHits.Should().Be(1);
        snap.LegacyHits.Should().Be(0);
        snap.FastPathHits.Should().Be(0);
        snap.FastPathSkipNoServices.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GivenCompiledPatternsDisabled_WhenResolving_ThenLegacyHitIncrements()
    {
        using var flags = new FeatureFlagScope(bitboards: false, compiledPatterns: false, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = false;
        FastPathMetrics.Reset();
        var builder = new RookNorthBuilder();
        var progress = builder.Compile();
        var rook = progress.Game.GetPiece("rook");
        var from = progress.Game.GetTile("v1");
        var to = progress.Game.GetTile("v3");
        var path = progress.ResolvePathCompiledFirst(rook, from, to);
        path.Should().NotBeNull();
        var snap = FastPathMetrics.Snapshot();
        snap.Attempts.Should().Be(1);
        snap.LegacyHits.Should().Be(1);
        snap.CompiledHits.Should().Be(0);
        snap.FastPathHits.Should().Be(0);
        snap.FastPathSkipNoServices.Should().BeGreaterThan(0);
    }

    // NOTE: FastPathSkipReconstructFail currently unreachable with consistent BoardShape invariants.
}