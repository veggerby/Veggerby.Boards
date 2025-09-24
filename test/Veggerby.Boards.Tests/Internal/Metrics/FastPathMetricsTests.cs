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

        var dirs = new[] { north, east, south, west, ne, nw, se, sw };
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
                if (r < 4)
                {
                    rels.Add(new TileRelation(T(c, r), T(c, r + 1), north));
                }

                if (r > 1)
                {
                    rels.Add(new TileRelation(T(c, r), T(c, r - 1), south));
                }

                if (c < 4)
                {
                    rels.Add(new TileRelation(T(c, r), T(c + 1, r), east));
                }

                if (c > 1)
                {
                    rels.Add(new TileRelation(T(c, r), T(c - 1, r), west));
                }

                if (c < 4 && r < 4)
                {
                    rels.Add(new TileRelation(T(c, r), T(c + 1, r + 1), ne));
                }

                if (c > 1 && r < 4)
                {
                    rels.Add(new TileRelation(T(c, r), T(c - 1, r + 1), nw));
                }

                if (c < 4 && r > 1)
                {
                    rels.Add(new TileRelation(T(c, r), T(c + 1, r - 1), se));
                }

                if (c > 1 && r > 1)
                {
                    rels.Add(new TileRelation(T(c, r), T(c - 1, r - 1), sw));
                }
            }
        }
        var board = new Board("fast-metrics-board", rels);
        var white = new Player("white");
        var rook = new Piece("rook", white, [new DirectionPattern(north, true)]);
        var game = new Game(board, [white], [rook]);
        var initial = GameState.New([new PieceState(rook, T(2, 2))]);

        // Services
        var services = new EngineServices();
        var shape = BoardShape.Build(board);
        services.Set(shape);

        if (bitboards)
        {
            // Piece map
            var pmLayout = PieceMapLayout.Build(game);
            var pmSnapshot = PieceMapSnapshot.Build(pmLayout, initial, shape);
            services.Set(new PieceMapServices(pmLayout, pmSnapshot));
            // Bitboards
            var bbLayout = BitboardLayout.Build(game);
            var bbSnapshot = BitboardSnapshot.Build(bbLayout, initial, shape);
            services.Set(new BitboardServices(bbLayout, bbSnapshot));
            // Sliding attacks
            var sliding = SlidingAttackGenerator.Build(shape);
            services.Set(new AttackGeneratorServices(sliding));
        }

        // Minimal phase graph: single always-active phase with no-op rule to satisfy engine requirements.
        var noopRule = new NoOpRule();
        var phase = GamePhase.New(1, "noop", new NullGameStateCondition(), noopRule);
        var engine = new GameEngine(game, phase, null, NullEvaluationObserver.Instance, services);
        PieceMapSnapshot pmSnap = null; BitboardSnapshot bbSnap = null;
        if (services.TryGet(out PieceMapServices pms))
        {
            pmSnap = pms.Snapshot;
        }

        if (services.TryGet(out BitboardServices bbs))
        {
            bbSnap = bbs.Snapshot;
        }

        return new GameProgress(engine, initial, null, pmSnap, bbSnap);
    }

    [Fact]
    public void GivenBitboardsEnabled_WhenResolvingSlidingPath_ThenFastPathHitIncrementsCounter()
    {
        // arrange
        using var flags = new FeatureFlagScope(bitboards: true, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();
        var progress = BuildProgress(bitboards: true);
        var rook = progress.Game.GetPiece("rook");
        var from = progress.State.GetStates<PieceState>().Single(ps => ps.Artifact == rook).CurrentTile;
        var to = progress.Game.GetTile("t2-4");

        // act
        var path = progress.ResolvePathCompiledFirst(rook, from, to);

        // assert
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
        // arrange
        using var flags = new FeatureFlagScope(bitboards: false, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();
        var progress = BuildProgress(bitboards: false);
        var rook = progress.Game.GetPiece("rook");
        var from = progress.State.GetStates<PieceState>().Single(ps => ps.Artifact == rook).CurrentTile;
        var to = progress.Game.GetTile("t2-4");

        // act
        var path = progress.ResolvePathCompiledFirst(rook, from, to);

        // assert
        path.Should().NotBeNull();
        var snap = FastPathMetrics.Snapshot();
        snap.Attempts.Should().Be(1);
        snap.FastPathHits.Should().Be(0);
        snap.FastPathSkippedNoPrereq.Should().Be(1); // aggregate still increments
        snap.FastPathSkipNoServices.Should().BeGreaterThan(0);
        (snap.CompiledHits + snap.LegacyHits).Should().Be(1);
    }

    [Fact]
    public void GivenNonSlider_WhenResolvingPath_ThenFastPathSkipNotSliderIncrements()
    {
        // arrange
        using var flags = new FeatureFlagScope(bitboards: true, compiledPatterns: true, boardShape: true);
        Veggerby.Boards.Internal.FeatureFlags.EnableSlidingFastPath = true;
        FastPathMetrics.Reset();

        // Build progress with a knight (non-sliding pattern) so fast path must skip.
        var north = new Direction("north");
        var east = new Direction("east");
        var a = new Tile("a"); var b = new Tile("b");
        var board = new Board("ns-board", new[] { new TileRelation(a, b, east) });
        var white = new Player("white");
        var knight = new Piece("knight", white, new IPattern[] { new FixedPattern(new[] { east }) }); // single-step fixed pattern
        var game = new Game(board, new[] { white }, new Artifact[] { knight });
        var state = GameState.New(new IArtifactState[] { new PieceState(knight, a) });
        var shape = BoardShape.Build(board);
        var services = new EngineServices();
        services.Set(shape);
        var pmLayout = PieceMapLayout.Build(game);
        var pmSnapshot = PieceMapSnapshot.Build(pmLayout, state, shape);
        services.Set(new PieceMapServices(pmLayout, pmSnapshot));
        var bbLayout = BitboardLayout.Build(game);
        var bbSnapshot = BitboardSnapshot.Build(bbLayout, state, shape);
        services.Set(new BitboardServices(bbLayout, bbSnapshot));
        var sliding = SlidingAttackGenerator.Build(shape);
        services.Set(new AttackGeneratorServices(sliding));
        var phase = GamePhase.New(1, "noop", new NullGameStateCondition(), new NoOpRule());
        var engine = new GameEngine(game, phase, null, NullEvaluationObserver.Instance, services);
        var progress = new GameProgress(engine, state, null, pmSnapshot, bbSnapshot);

        // act
        var path = progress.ResolvePathCompiledFirst(knight, a, b);

        // assert
        var snap = FastPathMetrics.Snapshot();
        snap.FastPathSkipNotSlider.Should().BeGreaterThan(0);
    }
}