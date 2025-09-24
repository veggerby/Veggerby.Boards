using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

internal sealed class TestPhaseControlEvent(string phaseId) : IPhaseControlGameEvent
{
    public string PhaseId { get; } = phaseId;
}

internal sealed class RecordingConditionPhase<TEvent> : IGameEventCondition<TEvent> where TEvent : IGameEvent
{
    public int Evaluations { get; private set; }
    public ConditionResponse Evaluate(GameEngine engine, GameState state, TEvent @event)
    {
        Evaluations++;
        return ConditionResponse.Valid;
    }
}

public class DecisionPlanEventFilteringPhaseTests
{
    private static GameProgress Build(RecordingConditionPhase<MovePieceGameEvent> moveCond,
        RecordingConditionPhase<RollDiceGameEvent<int>> rollCond,
        RecordingConditionPhase<TestPhaseControlEvent> phaseCond)
    {
        var builder = new PhaseFilteringGameBuilder(moveCond, rollCond, phaseCond);
        FeatureFlags.EnableDecisionPlan = true;
        FeatureFlags.EnableDecisionPlanGrouping = true;
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        return builder.Compile();
    }

    [Fact]
    public void GivenPhaseEvent_WhenFilteringEnabled_ThenOnlyPhaseRuleEvaluated()
    {
        // arrange
        var moveCond = new RecordingConditionPhase<MovePieceGameEvent>();
        var rollCond = new RecordingConditionPhase<RollDiceGameEvent<int>>();
        var phaseCond = new RecordingConditionPhase<TestPhaseControlEvent>();
        var progress = Build(moveCond, rollCond, phaseCond);
        var evt = new TestPhaseControlEvent("advance-phase");

        // act
        progress.HandleEvent(evt);

        // assert
        phaseCond.Evaluations.Should().BeGreaterThan(0);
        moveCond.Evaluations.Should().Be(0);
        rollCond.Evaluations.Should().Be(0);
    }

    [Fact]
    public void GivenMoveEvent_WhenFilteringEnabled_ThenPhaseRuleSkipped()
    {
        // arrange
        var moveCond = new RecordingConditionPhase<MovePieceGameEvent>();
        var rollCond = new RecordingConditionPhase<RollDiceGameEvent<int>>();
        var phaseCond = new RecordingConditionPhase<TestPhaseControlEvent>();
        var progress = Build(moveCond, rollCond, phaseCond);
        var piece = progress.Game.GetArtifacts<Piece>().First();
        var from = progress.Game.Board.Tiles.First();
        var relation = progress.Game.Board.TileRelations.FirstOrDefault(r => r.From == from) ?? progress.Game.Board.TileRelations.First();
        var path = new TilePath(new[] { relation });
        var evt = new MovePieceGameEvent(piece, path);

        // act
        progress.HandleEvent(evt);

        // assert
        moveCond.Evaluations.Should().BeGreaterThan(0);
        phaseCond.Evaluations.Should().Be(0);
        rollCond.Evaluations.Should().Be(0);
    }
}

internal sealed class PhaseFilteringGameBuilder(RecordingConditionPhase<MovePieceGameEvent> moveCond,
    RecordingConditionPhase<RollDiceGameEvent<int>> rollCond,
    RecordingConditionPhase<TestPhaseControlEvent> phaseCond) : GameBuilder
{
    private readonly RecordingConditionPhase<MovePieceGameEvent> _moveCond = moveCond;
    private readonly RecordingConditionPhase<RollDiceGameEvent<int>> _rollCond = rollCond;
    private readonly RecordingConditionPhase<TestPhaseControlEvent> _phaseCond = phaseCond;

    protected override void Build()
    {
        BoardId = "phase-filter-board";
        AddTile("a");
        AddTile("b").WithRelationTo("a").InDirection("d").WithDistance(1).Done();
        AddDirection("d");
        AddPlayer("p1");
        AddPiece("piece-1").WithOwner("p1").OnTile("a");
        AddDice("dice-1");

        AddGamePhase("move-phase")
            .If<NullGameStateCondition>()
            .Then()
            .ForEvent<MovePieceGameEvent>()
                .If(_ => _moveCond)
                .Then();

        AddGamePhase("roll-phase")
            .If<NullGameStateCondition>()
            .Then()
            .ForEvent<RollDiceGameEvent<int>>()
                .If(_ => _rollCond)
                .Then();

        AddGamePhase("phase-control-phase")
            .If<NullGameStateCondition>()
            .Then()
            .ForEvent<TestPhaseControlEvent>()
                .If(_ => _phaseCond)
                .Then();
    }
}