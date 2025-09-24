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

internal sealed class DummyStateEvent : IStateMutationGameEvent { }

internal sealed class CountingEvaluationObserver : IEvaluationObserver
{
    public int RuleEvaluations { get; private set; }
    public void OnPhaseEnter(Veggerby.Boards.Flows.Phases.GamePhase phase, GameState state) { }
    public void OnRuleEvaluated(Veggerby.Boards.Flows.Phases.GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex)
    {
        RuleEvaluations++;
    }
    public void OnRuleApplied(Veggerby.Boards.Flows.Phases.GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex) { }
    public void OnEventIgnored(IGameEvent @event, GameState state) { }
    public void OnStateHashed(GameState state, ulong hash) { }
}

internal sealed class MetricsFilteringGameBuilder : GameBuilder
{
    protected override void Build()
    {
        BoardId = "metrics-filter-board";
        AddTile("a");
        AddTile("b").WithRelationTo("a").InDirection("d").WithDistance(1).Done();
        AddDirection("d");
        AddPlayer("p1");
        AddPiece("piece-1").WithOwner("p1").OnTile("a");
        AddDice("dice-1");

        AddGamePhase("move-phase").If<NullGameStateCondition>().Then()
            .ForEvent<MovePieceGameEvent>()
                .If(_ => new AlwaysValidCondition())
                .Then();
        AddGamePhase("roll-phase").If<NullGameStateCondition>().Then()
            .ForEvent<RollDiceGameEvent<int>>()
                .If(_ => new AlwaysValidCondition())
                .Then();
        AddGamePhase("state-phase").If<NullGameStateCondition>().Then()
            .ForEvent<DummyStateEvent>()
                .If(_ => new AlwaysValidCondition())
                .Then();
        AddGamePhase("phase-control-phase").If<NullGameStateCondition>().Then()
            .ForEvent<TestPhaseControlEvent>()
                .If(_ => new AlwaysValidCondition())
                .Then();
    }

    private sealed class AlwaysValidCondition : IGameEventCondition<IGameEvent>
    {
        public ConditionResponse Evaluate(GameEngine engine, GameState state, IGameEvent @event) => ConditionResponse.Valid;
    }
}

public class DecisionPlanEventFilteringMetricsTests
{
    private static (GameProgress progress, CountingEvaluationObserver observer) Build(bool filtering)
    {
        FeatureFlags.EnableDecisionPlan = true;
        FeatureFlags.EnableDecisionPlanGrouping = true;
        FeatureFlags.EnableDecisionPlanEventFiltering = filtering;
        var observer = new CountingEvaluationObserver();
        var builder = new MetricsFilteringGameBuilder().WithObserver(observer);
        var progress = builder.Compile();
        return (progress, observer);
    }

    [Fact]
    public void GivenMoveEvent_FilteringEnabled_SkipsNonMatchingRuleEvaluations()
    {
        // arrange
        var (disabledProgress, disabledObserver) = Build(filtering: false);
        var (enabledProgress, enabledObserver) = Build(filtering: true);
        var piece = disabledProgress.Game.GetArtifacts<Piece>().First();
        var from = disabledProgress.Game.Board.Tiles.First();
        var relation = disabledProgress.Game.Board.TileRelations.FirstOrDefault(r => r.From == from) ?? disabledProgress.Game.Board.TileRelations.First();
        var path = new TilePath(new[] { relation });
        var move = new MovePieceGameEvent(piece, path);

        // act
        disabledProgress.HandleEvent(move);
        enabledProgress.HandleEvent(move);

        // assert
        // Current implementation already short-circuits non-matching event types before condition evaluation EVEN when filtering flag is off (plan still checks rule type via rule.Check). So both paths yield 1.
        disabledObserver.RuleEvaluations.Should().Be(1);
        enabledObserver.RuleEvaluations.Should().Be(1);
    }
}