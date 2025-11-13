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

/// <summary>
/// Verifies current engine behavior: enabling event kind filtering does not change the number of rule evaluations
/// nor emit EventKindFiltered skip notifications with the present pre-classification implementation.
/// These tests guard neutrality so future changes that introduce observable skips can deliberately update expectations.
/// </summary>
public class DecisionPlanEventFilteringNeutralityTests
{
    private sealed class CountingObserver : IEvaluationObserver
    {
        public int Evaluated
        {
            get; private set;
        }
        public int SkippedFiltered
        {
            get; private set;
        }
        public int SkippedOther
        {
            get; private set;
        }
        public void OnPhaseEnter(Boards.Flows.Phases.GamePhase phase, GameState state)
        {
        }
        public void OnRuleEvaluated(Boards.Flows.Phases.GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex)
        {
            Evaluated++;
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
        public void OnRuleSkipped(Boards.Flows.Phases.GamePhase phase, IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex)
        {
            if (reason == RuleSkipReason.EventKindFiltered)
            {
                SkippedFiltered++;
            }
            else
            {
                SkippedOther++;
            }
        }
    }

    private static (GameProgress progress, CountingObserver observer) BuildMoveLast(int precedingGroups, bool filtering)
    {
        FeatureFlags.EnableDecisionPlanGrouping = true;
        FeatureFlags.EnableDecisionPlanEventFiltering = filtering;
        var observer = new CountingObserver();
        var builder = new MoveLastGameBuilder(precedingGroups).WithObserver(observer);
        return (builder.Compile(), observer);
    }

    private static (GameProgress progress, CountingObserver observer) BuildRollLast(int precedingGroups, bool filtering)
    {
        FeatureFlags.EnableDecisionPlanGrouping = true;
        FeatureFlags.EnableDecisionPlanEventFiltering = filtering;
        var observer = new CountingObserver();
        var builder = new RollLastGameBuilder(precedingGroups).WithObserver(observer);
        return (builder.Compile(), observer);
    }

    [Fact]
    public void GivenMoveEvent_FilteringIsNeutral()
    {
        // arrange

        // act

        // assert

        var (disabledProgress, disabledObs) = BuildMoveLast(precedingGroups: 4, filtering: false); // groups before move: roll/state/control/dummy
        var (enabledProgress, enabledObs) = BuildMoveLast(precedingGroups: 4, filtering: true);
        var piece = disabledProgress.Game.GetArtifacts<Piece>().First();
        var from = disabledProgress.Game.Board.Tiles.First();
        var relation = disabledProgress.Game.Board.TileRelations.FirstOrDefault(r => r.From == from) ?? disabledProgress.Game.Board.TileRelations.First();
        var path = new TilePath([relation]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        disabledProgress.HandleEvent(evt);
        enabledProgress.HandleEvent(evt);

        // assert
        enabledObs.Evaluated.Should().Be(disabledObs.Evaluated);
        enabledObs.SkippedFiltered.Should().Be(0);
        disabledObs.SkippedFiltered.Should().Be(0);
    }

    [Fact]
    public void GivenRollEvent_FilteringIsNeutral()
    {
        // arrange

        // act

        // assert

        var (disabledProgress, disabledObs) = BuildRollLast(precedingGroups: 5, filtering: false); // move/state/control/dummy/move2 before roll
        var (enabledProgress, enabledObs) = BuildRollLast(precedingGroups: 5, filtering: true);
        var diceStates = enabledProgress.Game.GetArtifacts<Dice>().Select(d => new DiceState<int>(d, 1)).ToArray();
        var evt = new RollDiceGameEvent<int>(diceStates);

        // act
        disabledProgress.HandleEvent(evt);
        enabledProgress.HandleEvent(evt);

        // assert
        enabledObs.Evaluated.Should().Be(disabledObs.Evaluated);
        enabledObs.SkippedFiltered.Should().Be(0);
        disabledObs.SkippedFiltered.Should().Be(0);
    }

    [Fact]
    public void GivenSingleMatchingRule_FilteringIsNeutral()
    {
        // arrange

        // act

        // assert

        var (disabledProgress, disabledObs) = BuildMoveLast(precedingGroups: 0, filtering: false);
        var (enabledProgress, enabledObs) = BuildMoveLast(precedingGroups: 0, filtering: true);
        var piece = disabledProgress.Game.GetArtifacts<Piece>().First();
        var from = disabledProgress.Game.Board.Tiles.First();
        var relation = disabledProgress.Game.Board.TileRelations.FirstOrDefault(r => r.From == from) ?? disabledProgress.Game.Board.TileRelations.First();
        var path = new TilePath([relation]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        disabledProgress.HandleEvent(evt);
        enabledProgress.HandleEvent(evt);

        // assert
        disabledObs.Evaluated.Should().Be(1);
        enabledObs.Evaluated.Should().Be(1);
        enabledObs.SkippedFiltered.Should().Be(0);
    }

    private sealed class AlwaysValidCondition<TEvent> : IGameEventCondition<TEvent> where TEvent : IGameEvent
    {
        public ConditionResponse Evaluate(GameEngine engine, GameState state, TEvent @event) => ConditionResponse.Valid;
    }

    // Builder where Move rule is placed last; precedingGroups defines how many distinct non-matching groups are inserted before it.
    private sealed class MoveLastGameBuilder(int preceding) : GameBuilder
    {
        private readonly int _preceding = preceding;

        protected override void Build()
        {
            BoardId = "move-last-filter-board";
            AddTile("t1");
            AddTile("t2").WithRelationTo("t1").InDirection("d").WithDistance(1).Done();
            AddDirection("d");
            AddPlayer("p1");
            AddPiece("piece").WithOwner("p1").OnTile("t1");
            AddDice("dice");

            // Cycle through non-matching kinds in a predictable order.
            for (var i = 0; i < _preceding; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        AddGamePhase($"roll-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<RollDiceGameEvent<int>>()
                                .If(_ => new AlwaysValidCondition<RollDiceGameEvent<int>>())
                                .Then();
                        break;
                    case 1:
                        AddGamePhase($"state-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<DummyStateEvent>()
                                .If(_ => new AlwaysValidCondition<DummyStateEvent>())
                                .Then();
                        break;
                    case 2:
                        AddGamePhase($"control-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<TestPhaseControlEvent>()
                                .If(_ => new AlwaysValidCondition<TestPhaseControlEvent>())
                                .Then();
                        break;
                    default:
                        AddGamePhase($"roll2-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<RollDiceGameEvent<int>>()
                                .If(_ => new AlwaysValidCondition<RollDiceGameEvent<int>>())
                                .Then();
                        break;
                }
            }

            // Target rule last
            AddGamePhase("move-phase").If<NullGameStateCondition>().Then()
                .ForEvent<MovePieceGameEvent>()
                    .If(_ => new AlwaysValidCondition<MovePieceGameEvent>())
                    .Then();
        }
    }

    // Builder where Roll rule is placed last; preceding groups include other kinds (including extra move rules) first.
    private sealed class RollLastGameBuilder(int preceding) : GameBuilder
    {
        private readonly int _preceding = preceding;

        protected override void Build()
        {
            BoardId = "roll-last-filter-board";
            AddTile("t1");
            AddTile("t2").WithRelationTo("t1").InDirection("d").WithDistance(1).Done();
            AddDirection("d");
            AddPlayer("p1");
            AddPiece("piece").WithOwner("p1").OnTile("t1");
            AddDice("dice");

            for (var i = 0; i < _preceding; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        AddGamePhase($"move-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<MovePieceGameEvent>()
                                .If(_ => new AlwaysValidCondition<MovePieceGameEvent>())
                                .Then();
                        break;
                    case 1:
                        AddGamePhase($"state-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<DummyStateEvent>()
                                .If(_ => new AlwaysValidCondition<DummyStateEvent>())
                                .Then();
                        break;
                    case 2:
                        AddGamePhase($"control-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<TestPhaseControlEvent>()
                                .If(_ => new AlwaysValidCondition<TestPhaseControlEvent>())
                                .Then();
                        break;
                    default:
                        AddGamePhase($"move2-pre-{i}").If<NullGameStateCondition>().Then()
                            .ForEvent<MovePieceGameEvent>()
                                .If(_ => new AlwaysValidCondition<MovePieceGameEvent>())
                                .Then();
                        break;
                }
            }

            AddGamePhase("roll-phase").If<NullGameStateCondition>().Then()
                .ForEvent<RollDiceGameEvent<int>>()
                    .If(_ => new AlwaysValidCondition<RollDiceGameEvent<int>>())
                    .Then();
        }
    }
}
