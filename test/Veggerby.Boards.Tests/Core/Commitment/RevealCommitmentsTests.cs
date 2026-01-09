using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Flows.Rules.Conditions.Commitment;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Commitment;

/// <summary>
/// Tests for the reveal commitments functionality, including event application and determinism.
/// </summary>
public class RevealCommitmentsTests
{
    [Fact]
    public void GivenRevealCommitmentsEvent_WhenNoStagedEventsState_ThenNoOp()
    {
        // arrange
        var gameState = GameState.New([]);
        var revealEvent = new RevealCommitmentsEvent();
        var mutator = new RevealCommitmentsStateMutator();

        // act
        var newState = mutator.MutateState(null!, gameState, revealEvent);

        // assert
        newState.Should().Be(gameState);
    }

    [Fact]
    public void GivenRevealCommitmentsEvent_WhenCommitmentsNotComplete_ThenNoOp()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var pendingPlayers = new HashSet<Player> { player1 };
        var stagedState = new StagedEventsState(artifact, new Dictionary<Player, IGameEvent>(), pendingPlayers);

        var gameState = GameState.New([stagedState]);
        var revealEvent = new RevealCommitmentsEvent();
        var mutator = new RevealCommitmentsStateMutator();

        // act
        var newState = mutator.MutateState(null!, gameState, revealEvent);

        // assert
        newState.Should().Be(gameState);
    }

    [Fact]
    public void GivenRevealCommitmentsEvent_WhenAllCommitmentsComplete_ThenStagedEventsStateIsRemoved()
    {
        // arrange
        // Create a minimal game to test with
        var builder = new MinimalCommitmentGameBuilder();
        var progress = builder.Compile();

        // Add staged events state with complete commitments
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = progress.Game.Players.First();
        var commitments = new Dictionary<Player, IGameEvent>
        {
            [player1] = new NullGameEvent()
        };
        var stagedState = new StagedEventsState(artifact, commitments, new HashSet<Player>());

        var stateWithStaged = progress.State.Next([stagedState]);

        // act
        var resultProgress = new GameProgress(progress.Engine, stateWithStaged, null);
        var revealedProgress = resultProgress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        revealedProgress.State.GetStates<StagedEventsState>().Should().BeEmpty();
    }

    [Fact]
    public void GivenMultipleCommitments_WhenRevealed_ThenEventsAppliedInPlayerIdOrder()
    {
        // arrange
        var builder = new DeterministicOrderGameBuilder();
        var progress = builder.Compile();

        var playerZ = progress.Game.Players.First(p => p.Id == "player-z");
        var playerA = progress.Game.Players.First(p => p.Id == "player-a");
        var playerM = progress.Game.Players.First(p => p.Id == "player-m");

        // Commit in random order (z, a, m) but should apply in alphabetical order (a, m, z)
        var artifact = new StagedEventsArtifact("staged-events");
        var commitments = new Dictionary<Player, IGameEvent>
        {
            [playerZ] = new IncrementCounterEvent(playerZ),
            [playerA] = new IncrementCounterEvent(playerA),
            [playerM] = new IncrementCounterEvent(playerM)
        };
        var stagedState = new StagedEventsState(artifact, commitments, new HashSet<Player>());

        var stateWithStaged = progress.State.Next([stagedState]);

        // act
        var resultProgress = new GameProgress(progress.Engine, stateWithStaged, null);
        var revealedProgress = resultProgress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var counterState = revealedProgress.State.GetStates<CounterState>().FirstOrDefault();
        counterState.Should().NotBeNull();
        counterState!.OrderLog.Should().Equal("player-a", "player-m", "player-z");
    }

    [Fact]
    public void GivenCommittedEvents_WhenRevealed_ThenEventsAreActuallyApplied()
    {
        // arrange
        var builder = new DeterministicOrderGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");

        var artifact = new StagedEventsArtifact("staged-events");
        var commitments = new Dictionary<Player, IGameEvent>
        {
            [player1] = new IncrementCounterEvent(player1)
        };
        var stagedState = new StagedEventsState(artifact, commitments, new HashSet<Player>());

        var stateWithStaged = progress.State.Next([stagedState]);

        // act
        var resultProgress = new GameProgress(progress.Engine, stateWithStaged, null);
        var revealedProgress = resultProgress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var counterState = revealedProgress.State.GetStates<CounterState>().FirstOrDefault();
        counterState.Should().NotBeNull();
        counterState!.Count.Should().Be(1);
        counterState.OrderLog.Should().ContainSingle().Which.Should().Be("player-1");
    }
}

/// <summary>
/// Simple test event that increments a counter.
/// </summary>
internal sealed record IncrementCounterEvent(Player Player) : IGameEvent;

/// <summary>
/// Simple test artifact for counter tracking.
/// </summary>
internal sealed class CounterArtifact : Artifact
{
    public CounterArtifact(string id) : base(id) { }
}

/// <summary>
/// Simple test state to track event applications and order.
/// </summary>
internal sealed class CounterState : IArtifactState
{
    public Artifact Artifact { get; }
    public int Count { get; }
    public List<string> OrderLog { get; }

    public CounterState(Artifact artifact, int count, List<string> orderLog)
    {
        Artifact = artifact;
        Count = count;
        OrderLog = orderLog;
    }

    public bool Equals(IArtifactState? other)
    {
        return other is CounterState cs
            && Artifact.Equals(cs.Artifact)
            && Count == cs.Count;
    }

    public override bool Equals(object? obj) => Equals(obj as IArtifactState);
    public override int GetHashCode() => HashCode.Combine(Artifact, Count);
}

/// <summary>
/// Mutator that increments the counter state.
/// </summary>
internal sealed class IncrementCounterStateMutator : IStateMutator<IncrementCounterEvent>
{
    public GameState MutateState(GameEngine engine, GameState gameState, IncrementCounterEvent @event)
    {
        var currentCounter = gameState.GetStates<CounterState>().FirstOrDefault();
        var newCount = (currentCounter?.Count ?? 0) + 1;
        var newLog = new List<string>(currentCounter?.OrderLog ?? new List<string>());
        newLog.Add(@event.Player.Id);

        var counterArtifact = currentCounter?.Artifact ?? new CounterArtifact("counter");
        var newCounter = new CounterState(counterArtifact, newCount, newLog);

        return gameState.Next([newCounter]);
    }
}

/// <summary>
/// Condition that always returns Valid for test purposes.
/// </summary>
internal sealed class AlwaysValidIncrementCondition : IGameEventCondition<IncrementCounterEvent>
{
    public ConditionResponse Evaluate(GameEngine engine, GameState state, IncrementCounterEvent @event)
    {
        return ConditionResponse.Valid;
    }
}

/// <summary>
/// Minimal game builder for testing commitment/reveal mechanics.
/// </summary>
internal sealed class MinimalCommitmentGameBuilder : GameBuilder
{
    protected override void Build()
    {
        BoardId = "test-board";

        // Add a single player
        AddPlayer("player-1");

        // Add two tiles with a relation to satisfy board requirements
        AddDirection("forward");
        AddTile("tile-1").WithRelationTo("tile-2").InDirection("forward");
        AddTile("tile-2");

        // Add commitment phase
        AddGamePhase("commitment")
            .If<CommitmentPhaseActiveCondition>()
            .Then()
                .ForEvent<CommitActionEvent>()
                    .If<CommitActionCondition>()
                .Then()
                    .Do<CommitActionStateMutator>();

        // Add reveal phase
        AddGamePhase("reveal")
            .If<AllPlayersCommittedCondition>()
            .Then()
                .ForEvent<RevealCommitmentsEvent>()
                    .If<RevealCommitmentsCondition>()
                .Then()
                    .Do<RevealCommitmentsStateMutator>();
    }
}

/// <summary>
/// Game builder for testing deterministic ordering of committed events.
/// </summary>
internal sealed class DeterministicOrderGameBuilder : GameBuilder
{
    protected override void Build()
    {
        BoardId = "test-board";

        // Add multiple players with specific IDs for ordering tests
        AddPlayer("player-1");
        AddPlayer("player-z");
        AddPlayer("player-a");
        AddPlayer("player-m");

        // Add two tiles with a relation to satisfy board requirements
        AddDirection("forward");
        AddTile("tile-1").WithRelationTo("tile-2").InDirection("forward");
        AddTile("tile-2");

        // Add commitment phase
        AddGamePhase("commitment")
            .If<CommitmentPhaseActiveCondition>()
            .Then()
                .ForEvent<CommitActionEvent>()
                    .If<CommitActionCondition>()
                .Then()
                    .Do<CommitActionStateMutator>();

        // Add reveal phase
        AddGamePhase("reveal")
            .If<AllPlayersCommittedCondition>()
            .Then()
                .ForEvent<RevealCommitmentsEvent>()
                    .If<RevealCommitmentsCondition>()
                .Then()
                    .Do<RevealCommitmentsStateMutator>();

        // Add phase for handling increment events
        AddGamePhase("increment")
            .If<NullGameStateCondition>()
            .Then()
                .ForEvent<IncrementCounterEvent>()
                    .If<AlwaysValidIncrementCondition>()
                .Then()
                    .Do<IncrementCounterStateMutator>();
    }
}
