using System.Linq;
using System.Threading.Tasks;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Simulation;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Simulation;

/// <summary>
/// Advanced simulation tests for max depth termination, detailed batch metrics, and RNG seeding placeholder.
/// </summary>
public class SimulationAdvancedTests
{
    private static GameProgress BuildProgressWithMove()
    {
        var b = new MoveProgressBuilder();
        return b.Compile();
    }

    [Fact]
    public void GivenSequentialSimulator_WithMoveIntentPolicy_ThenTerminatesNoMoves()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgressWithMove();
        // Policy attempts to move piece from t1->t2; however relation added is t2 -> t1 (reverse) so FirstOrDefault from current
        // tile (t1) yields null immediately. Returning null at depth==0 results in terminal reason NoMoves (NOT PolicyReturnedNull).
        // PolicyReturnedNull is only used when depth > 0 (some progress or at least a prior policy invocation advanced depth)
        // and a subsequent null (or non-progress) occurs.
        var detailed = SequentialSimulator.RunDetailed(progress, state =>
        {
            var piece = progress.Game.GetArtifacts<Piece>().First();
            var pieceState = state.GetState<PieceState>(piece);
            var rel = progress.Game.Board.TileRelations.FirstOrDefault(r => r.From == pieceState.CurrentTile);
            if (rel is null)
            {
                return null; // no legal relation from current tile -> null policy (non-progress)
            }
            var path = new TilePath([rel]);
            return new MovePieceGameEvent(piece, path);
        }, maxDepth: 1);
        detailed.Result.TerminalReason.Should().Be(PlayoutTerminalReason.NoMoves);
        detailed.Metrics.AppliedEvents.Should().Be(0);
    }

    [Fact]
    public async Task GivenParallelDetailedSimulator_WhenMoveIntentPolicy_ThenNoProgressMetrics()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgressWithMove();
        PlayoutPolicy policy = state =>
        {
            var piece = progress.Game.GetArtifacts<Piece>().First();
            var pieceState = state.GetState<PieceState>(piece);
            var rel = progress.Game.Board.TileRelations.FirstOrDefault(r => r.From == pieceState.CurrentTile);
            if (rel is null)
            {
                return null; // depth still 0 in each playout -> NoMoves terminal reason
            }
            var path = new TilePath([rel]);
            return new MovePieceGameEvent(piece, path);
        };
        var batch = await ParallelSimulator.RunManyDetailedAsync(progress, 3, _ => policy, maxDepth: 1);
        batch.Basic.Results.Should().HaveCount(3);
        foreach (var r in batch.Basic.Results)
        {
            r.AppliedEvents.Should().Be(0);
            r.TerminalReason.Should().Be(PlayoutTerminalReason.NoMoves);
        }
        foreach (var m in batch.Metrics)
        {
            m.AppliedEvents.Should().Be(0);
            m.PolicyCalls.Should().Be(1);
        }
        batch.TotalApplied.Should().Be(0);
    }

    [Fact]
    public async Task GivenParallelSimulator_WithDeterministicMoveIntentPolicy_ThenIdenticalFinalHashes()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgressWithMove();
        PlayoutPolicy policy = state =>
        {
            var piece = progress.Game.GetArtifacts<Piece>().First();
            var pieceState = state.GetState<PieceState>(piece);
            var rel = progress.Game.Board.TileRelations.FirstOrDefault(r => r.From == pieceState.CurrentTile);
            if (rel is null)
            {
                return null;
            }
            var path = new TilePath([rel]);
            return new MovePieceGameEvent(piece, path);
        };
        var batch = await ParallelSimulator.RunManyAsync(progress, 4, _ => policy, maxDepth: 1);
        foreach (var r in batch.Results)
        {
            r.AppliedEvents.Should().Be(0);
            r.TerminalReason.Should().Be(PlayoutTerminalReason.NoMoves);
        }
        var hashes = batch.Results.Select(r => r.Final.State.GetHashCode()).ToArray();
        hashes.Distinct().Should().HaveCount(1);
        // Placeholder: when per-index RNG seeding introduced, this test can branch to assert distribution divergence.
    }

    private sealed class MoveProgressBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "sim-advanced-board";
            AddTile("t1");
            AddTile("t2").WithRelationTo("t1").InDirection("d").WithDistance(1).Done();
            AddDirection("d");
            AddPlayer("p1");
            AddPiece("piece-1").WithOwner("p1").OnTile("t1");
            AddGamePhase("move-phase")
                .If<NullGameStateCondition>()
                .Then()
                .ForEvent<MovePieceGameEvent>()
                    .If(_ => new AlwaysValid<MovePieceGameEvent>())
                    .Then();
        }

        private sealed class AlwaysValid<TEvent> : IGameEventCondition<TEvent> where TEvent : IGameEvent
        {
            public ConditionResponse Evaluate(GameEngine engine, GameState state, TEvent @event) => ConditionResponse.Valid;
        }
    }
}