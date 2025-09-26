using System.Linq;

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
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();
        // policy always selects the single forward relation (t1->t2) and emits a move
        var detailed = SequentialSimulator.RunDetailed(progress, state =>
        {
            var piece = progress.Game.GetArtifacts<Piece>().First();
            var pieceState = state.GetState<PieceState>(piece);
            var rel = progress.Game.Board.TileRelations.First(r => r.From == pieceState.CurrentTile);
            var path = new TilePath([rel]);
            return new MovePieceGameEvent(piece, path);
        }, maxDepth: 1);

        detailed.Result.TerminalReason.Should().Be(PlayoutTerminalReason.MaxDepth);
        detailed.Metrics.AppliedEvents.Should().Be(1);
        detailed.Metrics.PolicyCalls.Should().Be(1);
        detailed.Result.Final.State.GetState<PieceState>(progress.Game.GetArtifacts<Piece>().First()).CurrentTile.Id.Should().Be("t2");
    }

    [Fact]
    public void GivenSequentialSimulator_WhenPolicyReturnsNullButMovesExist_ThenPolicyReturnedNull()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();
        // Policy applies exactly one legal move (t1->t2) and then returns null on the next invocation.
        // This exercises the distinction:
        //  - First call: move applied (depth becomes 1)
        //  - Second call: returns null with depth > 0 -> terminal reason PolicyReturnedNull
        var call = 0;
        var detailed = SequentialSimulator.RunDetailed(progress, state =>
        {
            if (call == 0)
            {
                call++;
                var piece = progress.Game.GetArtifacts<Piece>().First();
                var pieceState = state.GetState<PieceState>(piece);
                var rel = progress.Game.Board.TileRelations.First(r => r.From == pieceState.CurrentTile);
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