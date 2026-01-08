using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions.Commitment;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Commitment;

/// <summary>
/// Tests for the reveal commitments functionality, including event application and determinism.
/// </summary>
public class RevealCommitmentsTests
{
    /// <summary>
    /// Simple test event that increments a counter in ExtrasState.
    /// </summary>
    private sealed record IncrementCounterEvent(Player Player) : IGameEvent;

    /// <summary>
    /// Simple test state to track event applications.
    /// </summary>
    private sealed class CounterState : IArtifactState
    {
        public Artifact Artifact { get; }
        public int Count { get; }

        public CounterState(Artifact artifact, int count)
        {
            Artifact = artifact;
            Count = count;
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
