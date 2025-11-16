using System.Linq;
using System.Threading.Tasks;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Simulation;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Simulation;

/// <summary>
/// Tests covering progressing move semantics (MaxDepth) and PolicyReturnedNull when legal moves exist but policy yields null.
/// </summary>
public class SimulationProgressingTests
{
    private static GameProgress BuildProgress()
    {
        var b = new ProgressingMoveBuilder();
        return b.Compile();
    }

    [Fact]
    public void GivenSequentialSimulator_WhenProgressingMovePolicyAndMaxDepth1_ThenMaxDepthAndOneApplied()
    {
        // arrange

        // act

        // assert

        var progress = BuildProgress();
        // Policy always selects the single forward relation (t1->t2) and emits exactly one move.
        // Depth accounting:
        //  - Before any policy call: depth = 0
        //  - After first applied move: depth = 1 which equals maxDepth => terminal reason MaxDepth (NOT PolicyReturnedNull)
        // This verifies the simulator prefers MaxDepth over other terminal reasons when depth reaches the cap right after applying a move.
        var detailed = SequentialSimulator.RunDetailed(progress, state =>
        {
            var piece = progress.Game.GetArtifacts<Piece>().First();
            piece.Should().NotBeNull();
            var pieceState = state.GetState<PieceState>(piece);
            pieceState.Should().NotBeNull();
            var rel = progress.Game.Board.TileRelations.First(r => r.From == pieceState.CurrentTile);
            rel.Should().NotBeNull();
            var path = new TilePath([rel]);
            return new MovePieceGameEvent(piece, path);
        }, maxDepth: 1);

        detailed.Result.TerminalReason.Should().Be(PlayoutTerminalReason.MaxDepth);
        detailed.Metrics.AppliedEvents.Should().Be(1);
        detailed.Metrics.PolicyCalls.Should().Be(1);
        var finalPiece = progress.Game.GetArtifacts<Piece>().First();
        finalPiece.Should().NotBeNull();
        var finalPieceState = detailed.Result.Final.State.GetState<PieceState>(finalPiece);
        finalPieceState.Should().NotBeNull();
        finalPieceState!.CurrentTile.Should().NotBeNull();
        finalPieceState!.CurrentTile!.Id.Should().Be("t2");
    }

    [Fact]
    public void GivenSequentialSimulator_WhenPolicyReturnsNullButMovesExist_ThenPolicyReturnedNull()
    {
        // arrange

        // act

        // assert

        var progress = BuildProgress();
        // Policy applies exactly one legal move (t1->t2) and then returns null on the next invocation.
        // Distinction logic:
        //  - First call: move applied (depth becomes 1 < maxDepth -> continue)
        //  - Second call: returns null while depth > 0; simulator emits PolicyReturnedNull (NOT NoMoves)
        // NoMoves only occurs when first policy invocation (depth = 0) returns null (no progress at all).
        var call = 0;
        var detailed = SequentialSimulator.RunDetailed(progress, state =>
        {
            if (call == 0)
            {
                call++;
                var piece = progress.Game.GetArtifacts<Piece>().First();
                piece.Should().NotBeNull();
                var pieceState = state.GetState<PieceState>(piece);
                pieceState.Should().NotBeNull();
                var rel = progress.Game.Board.TileRelations.First(r => r.From == pieceState.CurrentTile);
                rel.Should().NotBeNull();
                var path = new TilePath([rel]);
                return new MovePieceGameEvent(piece, path);
            }
            // subsequent invocation returns null despite no moves from t2 (even if moves existed, depth>0 qualifies for PolicyReturnedNull)
            call++;
            return null;
        }, maxDepth: 5);
        detailed.Result.TerminalReason.Should().Be(PlayoutTerminalReason.PolicyReturnedNull);
        detailed.Metrics.AppliedEvents.Should().Be(1);
        detailed.Metrics.PolicyCalls.Should().Be(2);
    }

    [Fact]
    public async Task GivenParallelSimulator_WhenProgressingMovePolicyAndMaxDepth1_ThenEachPlayoutMaxDepthAndOneApplied()
    {
        // arrange

        // act

        // assert

        var progress = BuildProgress();
        PlayoutPolicy policy = state =>
        {
            var piece = progress.Game.GetArtifacts<Piece>().First();
            piece.Should().NotBeNull();
            var pieceState = state.GetState<PieceState>(piece);
            pieceState.Should().NotBeNull();
            var rel = progress.Game.Board.TileRelations.First(r => r.From == pieceState.CurrentTile);
            rel.Should().NotBeNull();
            var path = new TilePath([rel]);
            return new MovePieceGameEvent(piece, path);
        };
        var batch = await ParallelSimulator.RunManyDetailedAsync(progress, playoutCount: 3, policyFactory: _ => policy, maxDepth: 1);
        batch.Basic.Results.Should().HaveCount(3);
        foreach (var r in batch.Basic.Results)
        {
            r.TerminalReason.Should().Be(PlayoutTerminalReason.MaxDepth);
            r.AppliedEvents.Should().Be(1);
        }
        foreach (var m in batch.Metrics)
        {
            m.AppliedEvents.Should().Be(1);
            m.PolicyCalls.Should().Be(1);
        }
        batch.TotalApplied.Should().Be(3);
    }

    [Fact]
    public async Task GivenParallelSimulator_WhenPolicyReturnsNullAfterProgress_ThenPolicyReturnedNull()
    {
        // arrange

        // act

        // assert

        var progress = BuildProgress();
        var batch = await ParallelSimulator.RunManyDetailedAsync(progress, playoutCount: 2, policyFactory: _ =>
        {
            var piece = progress.Game.GetArtifacts<Piece>().First();
            int call = 0;
            return state =>
            {
                if (call == 0)
                {
                    call++;
                    var pieceState = state.GetState<PieceState>(piece);
                    pieceState.Should().NotBeNull();
                    var rel = progress.Game.Board.TileRelations.First(r => r.From == pieceState.CurrentTile);
                    rel.Should().NotBeNull();
                    var path = new TilePath([rel]);
                    return new MovePieceGameEvent(piece, path);
                }
                call++;
                // Returning null after at least one applied event -> PolicyReturnedNull (same semantics as sequential test)
                return null;
            };
        }, maxDepth: 5);

        foreach (var r in batch.Basic.Results)
        {
            r.TerminalReason.Should().Be(PlayoutTerminalReason.PolicyReturnedNull);
            r.AppliedEvents.Should().Be(1);
        }
        foreach (var m in batch.Metrics)
        {
            m.AppliedEvents.Should().Be(1);
            m.PolicyCalls.Should().Be(2);
        }
        batch.TotalApplied.Should().Be(2);
    }

    [Fact]
    public void GivenSequentialSimulator_WhenPolicyEmitsIllegalMove_ThenPolicyReturnedNullAndRejectedIncremented()
    {
        // arrange

        // act

        // assert

        var progress = BuildProgress();
        // Rejection scenario: first policy call emits a valid MovePieceGameEvent (applies). Second call emits a TurnPassEvent
        // for which there is no rule in this phase, resulting in no state change (rejected). The simulator treats the
        // absence of progress (after a prior progress) as PolicyReturnedNull and increments RejectedEvents. This clarifies
        // that rejection does not map to a distinct terminal reason; it influences metrics only.
        var piece = progress.Game.GetArtifacts<Piece>().First();
        int calls = 0;
        var detailed = SequentialSimulator.RunDetailed(progress, state =>
        {
            if (calls == 0)
            {
                calls++;
                var pieceState = state.GetState<PieceState>(piece);
                pieceState.Should().NotBeNull();
                var rel = progress.Game.Board.TileRelations.First(r => r.From == pieceState.CurrentTile);
                rel.Should().NotBeNull();
                return new MovePieceGameEvent(piece, new TilePath([rel]));
            }
            calls++;
            return new TurnPassEvent(); // unhandled -> rejected
        }, maxDepth: 4);
        detailed.Result.TerminalReason.Should().Be(PlayoutTerminalReason.PolicyReturnedNull);
        detailed.Metrics.AppliedEvents.Should().Be(1);
        detailed.Metrics.PolicyCalls.Should().Be(2);
        detailed.Metrics.RejectedEvents.Should().Be(1);
    }

    private sealed class ProgressingMoveBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "progressing-sim-board";
            AddTile("t1");
            AddTile("t2").WithRelationFrom("t1").InDirection("d").WithDistance(1).Done(); // create forward relation t1->t2
            AddDirection("d");
            AddPlayer("p1");
            AddPiece("piece").WithOwner("p1").OnTile("t1");
            AddGamePhase("move-phase")
                .If<NullGameStateCondition>()
                .Then()
                .ForEvent<MovePieceGameEvent>()
                    .If(_ => new AlwaysValid<MovePieceGameEvent>())
                    .Then()
                        .Do<MovePieceStateMutator>();
        }

        private sealed class AlwaysValid<TEvent> : IGameEventCondition<TEvent> where TEvent : IGameEvent
        {
            public ConditionResponse Evaluate(GameEngine engine, GameState state, TEvent @event) => ConditionResponse.Valid;
        }
    }
}
