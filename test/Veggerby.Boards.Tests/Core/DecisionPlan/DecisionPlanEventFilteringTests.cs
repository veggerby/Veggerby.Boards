using System.Linq;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Xunit;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

internal sealed class RecordingCondition<TEvent> : IGameEventCondition<TEvent> where TEvent : IGameEvent
{
    public int Evaluations { get; private set; }
    public ConditionResponse Evaluate(GameEngine engine, GameState state, TEvent @event)
    {
        Evaluations++;
        return ConditionResponse.Valid;
    }
}

public class DecisionPlanEventFilteringTests
{
    private static GameProgress BuildProgressWithPhases(RecordingCondition<MovePieceGameEvent> moveCond, RecordingCondition<RollDiceGameEvent<int>> rollCond)
    {
        // Minimal concrete builder
        var builder = new TestFilteringGameBuilder(moveCond, rollCond);
        FeatureFlags.EnableDecisionPlan = true;
        FeatureFlags.EnableDecisionPlanGrouping = true; // ensure groups present
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        return builder.Compile();
    }

    [Fact]
    public void GivenMoveEvent_WhenFilteringEnabled_ThenRollRuleSkipped()
    {
        // arrange
    var moveCond = new RecordingCondition<MovePieceGameEvent>();
    var rollCond = new RecordingCondition<RollDiceGameEvent<int>>();
    var progress = BuildProgressWithPhases(moveCond, rollCond);
    var piece = progress.Game.GetArtifacts<Piece>().First();
    // Build a trivial path (single relation) from tile A to tile A (self-loop) using existing board relation.
    var from = progress.Game.Board.Tiles.First();
    var relation = progress.Game.Board.TileRelations.FirstOrDefault(r => r.From == from) ?? progress.Game.Board.TileRelations.First();
    var path = new TilePath(new [] { relation });
    var evt = new MovePieceGameEvent(piece, path);

        // act
        progress.HandleEvent(evt);

        // assert
        moveCond.Evaluations.Should().BeGreaterThan(0);
        rollCond.Evaluations.Should().Be(0); // filtered out before rule condition evaluation
    }

    [Fact]
    public void GivenRollEvent_WhenFilteringEnabled_ThenMoveRuleSkipped()
    {
        // arrange
    var moveCond = new RecordingCondition<MovePieceGameEvent>();
    var rollCond = new RecordingCondition<RollDiceGameEvent<int>>();
    var progress = BuildProgressWithPhases(moveCond, rollCond);
    var diceStates = progress.Game.GetArtifacts<Dice>().Select(d => new DiceState<int>(d, 1)).ToArray();
    var evt = new RollDiceGameEvent<int>(diceStates);

        // act
        progress.HandleEvent(evt);

        // assert
        rollCond.Evaluations.Should().BeGreaterThan(0);
        moveCond.Evaluations.Should().Be(0);
    }

    internal sealed class AlwaysValidTestCondition : IGameStateCondition
    {
        public ConditionResponse Evaluate(GameState state) => ConditionResponse.Valid;
    }
}

internal sealed class TestFilteringGameBuilder : GameBuilder
{
    private readonly RecordingCondition<MovePieceGameEvent> _moveCond;
    private readonly RecordingCondition<RollDiceGameEvent<int>> _rollCond;
    public TestFilteringGameBuilder(RecordingCondition<MovePieceGameEvent> moveCond, RecordingCondition<RollDiceGameEvent<int>> rollCond)
    {
        _moveCond = moveCond;
        _rollCond = rollCond;
    }

    protected override void Build()
    {
        BoardId = "test-board";
        // minimal two tiles and a relation to allow a path
        AddTile("tile-a");
        AddTile("tile-b").WithRelationTo("tile-a").InDirection("dir-1").WithDistance(1).Done();
        AddDirection("dir-1");
        AddPlayer("p1");
        AddPiece("piece-1").WithOwner("p1").OnTile("tile-a");
        // dice for roll event
        AddDice("dice-1");

        // phases share always-valid condition so they group; they host distinct rules
        // root phase via definitions (numbering assigned in compile)
        // Attach rules via ForEvent but we want our pre-built rule instances.
        // Simplest approach: create phases with trivial rules replaced post-build is non-trivial, so instead
        // we replicate semantics by wrapping our RecordingRule inside a delegating rule mutator chain.
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
    }
}
