using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.RulePriorityTests;

/// <summary>
/// Tests for rule priority and conflict resolution strategies.
/// </summary>
public class RulePriorityConflictResolutionTests
{
    private class TestEvent : IGameEvent
    {
    }

    private class TestMutator(string marker) : IStateMutator<IGameEvent>
    {
        public string Marker
        {
            get;
        } = marker;

        public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
        {
            // Add a marker artifact state to track which mutator was applied
            var artifact = new ExtrasArtifact($"marker-{Marker}");
            var extras = new ExtrasState(artifact, Marker, typeof(string));
            return state.Next([extras]);
        }
    }

    private class TestRule(IStateMutator<IGameEvent> mutator) : IGameEventRule
    {
        public ConditionResponse Check(GameEngine engine, GameState gameState, IGameEvent @event)
        {
            return @event is TestEvent ? ConditionResponse.Valid : ConditionResponse.NotApplicable;
        }

        public GameState HandleEvent(GameEngine engine, GameState gameState, IGameEvent @event)
        {
            return mutator.MutateState(engine, gameState, @event);
        }
    }

    [Fact]
    public void GivenFirstWinsStrategy_WhenMultipleRulesMatch_ThenFirstRuleApplied()
    {
        // arrange
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        GamePhase.New(
            1,
            "rule-a",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("A")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.FirstWins);
        GamePhase.New(
            2,
            "rule-b",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("B")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.FirstWins);

        var game = CreateMinimalGame();
        var engine = new GameEngine(game, root, global::Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root));
        var initialState = GameState.New([]);
        var progress = new GameProgress(engine, initialState, EventChain.Empty);

        // act
        var result = progress.HandleEvent(new TestEvent());

        // assert
        var marker = result.State.GetExtras<string>();
        marker.Should().Be("A"); // First rule should win
    }

    [Fact]
    public void GivenHighestPriorityStrategy_WhenMultipleRulesMatch_ThenHighestPriorityRuleApplied()
    {
        // arrange
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        GamePhase.New(
            1,
            "rule-low",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("Low")),
            root,
            priority: RulePriority.Low,
            conflictResolution: ConflictResolutionStrategy.HighestPriority);
        GamePhase.New(
            2,
            "rule-high",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("High")),
            root,
            priority: RulePriority.High,
            conflictResolution: ConflictResolutionStrategy.HighestPriority);
        GamePhase.New(
            3,
            "rule-normal",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("Normal")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.HighestPriority);

        var game = CreateMinimalGame();
        var engine = new GameEngine(game, root, global::Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root));
        var initialState = GameState.New([]);
        var progress = new GameProgress(engine, initialState, EventChain.Empty);

        // act
        var result = progress.HandleEvent(new TestEvent());

        // assert
        var marker = result.State.GetExtras<string>();
        marker.Should().Be("High"); // Highest priority rule should win
    }

    [Fact]
    public void GivenHighestPriorityStrategy_WhenSamePriority_ThenFirstDeclaredRuleWins()
    {
        // arrange
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        GamePhase.New(
            1,
            "rule-a",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("A")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.HighestPriority);
        GamePhase.New(
            2,
            "rule-b",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("B")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.HighestPriority);

        var game = CreateMinimalGame();
        var engine = new GameEngine(game, root, global::Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root));
        var initialState = GameState.New([]);
        var progress = new GameProgress(engine, initialState, EventChain.Empty);

        // act
        var result = progress.HandleEvent(new TestEvent());

        // assert
        var marker = result.State.GetExtras<string>();
        marker.Should().Be("A"); // First rule in declaration order should win for ties
    }

    [Fact]
    public void GivenLastWinsStrategy_WhenMultipleRulesMatch_ThenLastRuleApplied()
    {
        // arrange
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        GamePhase.New(
            1,
            "rule-a",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("A")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.LastWins);
        GamePhase.New(
            2,
            "rule-b",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("B")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.LastWins);
        GamePhase.New(
            3,
            "rule-c",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("C")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.LastWins);

        var game = CreateMinimalGame();
        var engine = new GameEngine(game, root, global::Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root));
        var initialState = GameState.New([]);
        var progress = new GameProgress(engine, initialState, EventChain.Empty);

        // act
        var result = progress.HandleEvent(new TestEvent());

        // assert
        var marker = result.State.GetExtras<string>();
        marker.Should().Be("C"); // Last rule should win
    }

    [Fact]
    public void GivenExclusiveStrategy_WhenMultipleRulesMatch_ThenThrowsException()
    {
        // arrange
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        GamePhase.New(
            1,
            "rule-a",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("A")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.Exclusive);
        GamePhase.New(
            2,
            "rule-b",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("B")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.Exclusive);

        var game = CreateMinimalGame();
        var engine = new GameEngine(game, root, global::Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root));
        var initialState = GameState.New([]);
        var progress = new GameProgress(engine, initialState, EventChain.Empty);

        // act & assert
        var act = () => progress.HandleEvent(new TestEvent());
        act.Should().Throw<BoardException>()
            .WithMessage("Exclusive conflict resolution failed*");
    }

    [Fact]
    public void GivenExclusiveStrategy_WhenSingleRuleMatches_ThenSucceeds()
    {
        // arrange
        var root = GamePhase.NewParent(100, "root", new NullGameStateCondition());
        GamePhase.New(
            1,
            "rule-a",
            new NullGameStateCondition(),
            new TestRule(new TestMutator("A")),
            root,
            priority: RulePriority.Normal,
            conflictResolution: ConflictResolutionStrategy.Exclusive);

        var game = CreateMinimalGame();
        var engine = new GameEngine(game, root, global::Veggerby.Boards.Flows.DecisionPlan.DecisionPlan.Compile(root));
        var initialState = GameState.New([]);
        var progress = new GameProgress(engine, initialState, EventChain.Empty);

        // act
        var result = progress.HandleEvent(new TestEvent());

        // assert
        var marker = result.State.GetExtras<string>();
        marker.Should().Be("A");
    }

    private static Game CreateMinimalGame()
    {
        // Create a minimal board with one tile and one self-relation
        var tile = new Tile("t1");
        var direction = new Direction("d1");
        var relation = new TileRelation(tile, tile, direction);
        var board = new Board("test-board", [relation]);
        var player = new Player("p1");
        var piece = new Piece("piece1", player, [], null);
        return new Game(board, [player], [piece]);
    }
}
