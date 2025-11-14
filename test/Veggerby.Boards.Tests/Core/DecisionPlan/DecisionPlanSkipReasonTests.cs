using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

// Simple test events
internal sealed class SkipReasonNoOpStateEvent : IStateMutationGameEvent
{
}

internal sealed class SkipReasonBuilder : GameBuilder
{
    protected override void Build()
    {
        BoardId = "skip-reason-board";
        AddTile("a");
        AddTile("b").WithRelationTo("a").InDirection("d").WithDistance(1).Done();
        AddDirection("d");
        AddPlayer("p1");
        AddPiece("piece-1").WithOwner("p1").OnTile("a");

        // Phase 1: Gate condition invalid to trigger GroupGateFailed when grouping enabled.
        AddGamePhase("invalid-group-phase").If<AlwaysInvalidStateCondition>().Then()
            .ForEvent<MovePieceGameEvent>()
                .If(_ => new AlwaysValidCondition())
                .Then();

        // Phase 2: State rule (event kind mismatch for Move events) -> EventKindFiltered.
        AddGamePhase("state-phase").If<NullGameStateCondition>().Then()
            .ForEvent<SkipReasonNoOpStateEvent>()
                .If(_ => new AlwaysValidCondition())
                .Then();

        // Phase 3 & 4: Two move phases in same exclusivity group to trigger ExclusivityMasked after first applies.
        AddGamePhase("move-phase-1").Exclusive("mv").If<NullGameStateCondition>().Then()
            .ForEvent<MovePieceGameEvent>()
                .If(_ => new AlwaysValidCondition())
                .Then();
        AddGamePhase("move-phase-2").Exclusive("mv").If<NullGameStateCondition>().Then()
            .ForEvent<MovePieceGameEvent>()
                .If(_ => new AlwaysValidCondition())
                .Then();
    }

    private sealed class AlwaysValidCondition : IGameEventCondition<IGameEvent>
    {
        public ConditionResponse Evaluate(GameEngine engine, GameState state, IGameEvent @event) => ConditionResponse.Valid;
    }

    private sealed class AlwaysInvalidStateCondition : IGameStateCondition
    {
        public ConditionResponse Evaluate(GameEngine engine, GameState state) => ConditionResponse.Invalid;
        public ConditionResponse Evaluate(GameState state) => ConditionResponse.Invalid;
    }
}

internal sealed class CapturingSkipObserver : IEvaluationObserver
{
    public List<RuleSkipReason> Reasons { get; } = new();

    public void OnPhaseEnter(Boards.Flows.Phases.GamePhase phase, GameState state)
    {
    }
    public void OnRuleEvaluated(Boards.Flows.Phases.GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex)
    {
    }
    public void OnRuleApplied(Boards.Flows.Phases.GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex)
    {
    }
    public void OnEventIgnored(IGameEvent @event, GameState state)
    {
    }
    public void OnStateHashed(GameState state, ulong hash)
    {
    }
    public void OnRuleSkipped(Boards.Flows.Phases.GamePhase phase, IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex) => Reasons.Add(reason);
}

public class DecisionPlanSkipReasonTests
{
    private static (GameProgress progress, CapturingSkipObserver observer, Piece piece, TilePath path) Build()
    {
        // DecisionPlan always enabled

        var observer = new CapturingSkipObserver();
        var builder = new SkipReasonBuilder().WithObserver(observer);
        var progress = builder.Compile();
        var piece = progress.Game.GetArtifacts<Piece>().First();
        var from = progress.Game.Board.Tiles.First();
        var relation = progress.Game.Board.TileRelations.First();
        var path = new TilePath([relation]);
        return (progress, observer, piece, path);
    }

    [Fact]
    public void WhenHandlingMove_GroupGateFailed_SkipReason_IsCaptured()
    {
        // arrange

        // act

        // assert

        var (progress, observer, piece, path) = Build();
        var move = new MovePieceGameEvent(piece, path);

        // act
        progress.HandleEvent(move);

        // assert
        observer.Reasons.Should().Contain(RuleSkipReason.GroupGateFailed);
    }
}
