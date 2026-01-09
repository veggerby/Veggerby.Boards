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

public class CommitmentMechanicsTests
{
    [Fact]
    public void GivenCommitActionEvent_WhenStagedEventsStateExists_ThenCommitmentIsRecorded()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var pendingPlayers = new HashSet<Player> { player1 };
        var stagedState = new StagedEventsState(artifact, new Dictionary<Player, IGameEvent>(), pendingPlayers);

        var gameState = GameState.New([stagedState]);
        var action = new NullGameEvent();
        var commitEvent = new CommitActionEvent(player1, action);

        var mutator = new CommitActionStateMutator();

        // act
        var newState = mutator.MutateState(null!, gameState, commitEvent);

        // assert
        var updatedStaged = newState.GetStates<StagedEventsState>().First();
        updatedStaged.Commitments.Should().ContainKey(player1);
        updatedStaged.Commitments[player1].Should().Be(action);
        updatedStaged.PendingPlayers.Should().NotContain(player1);
    }

    [Fact]
    public void GivenCommitActionEvent_WhenNoStagedEventsState_ThenNoOp()
    {
        // arrange
        var gameState = GameState.New([]);
        var player1 = new Player("player-1");
        var action = new NullGameEvent();
        var commitEvent = new CommitActionEvent(player1, action);

        var mutator = new CommitActionStateMutator();

        // act
        var newState = mutator.MutateState(null!, gameState, commitEvent);

        // assert
        newState.Should().Be(gameState);
    }

    [Fact]
    public void GivenCommitActionCondition_WhenPlayerInPendingSet_ThenReturnsValid()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var pendingPlayers = new HashSet<Player> { player1 };
        var stagedState = new StagedEventsState(artifact, new Dictionary<Player, IGameEvent>(), pendingPlayers);

        var gameState = GameState.New([stagedState]);
        var action = new NullGameEvent();
        var commitEvent = new CommitActionEvent(player1, action);

        var condition = new CommitActionCondition();

        // act
        var response = condition.Evaluate(null!, gameState, commitEvent);

        // assert
        response.Should().Be(ConditionResponse.Valid);
    }

    [Fact]
    public void GivenCommitActionCondition_WhenPlayerAlreadyCommitted_ThenReturnsInvalid()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var commitments = new Dictionary<Player, IGameEvent> { [player1] = new NullGameEvent() };
        var pendingPlayers = new HashSet<Player>();
        var stagedState = new StagedEventsState(artifact, commitments, pendingPlayers);

        var gameState = GameState.New([stagedState]);
        var action = new NullGameEvent();
        var commitEvent = new CommitActionEvent(player1, action);

        var condition = new CommitActionCondition();

        // act
        var response = condition.Evaluate(null!, gameState, commitEvent);

        // assert
        response.Should().Be(ConditionResponse.Invalid);
    }

    [Fact]
    public void GivenCommitActionCondition_WhenNoStagedEventsState_ThenReturnsNotApplicable()
    {
        // arrange
        var gameState = GameState.New([]);
        var player1 = new Player("player-1");
        var action = new NullGameEvent();
        var commitEvent = new CommitActionEvent(player1, action);

        var condition = new CommitActionCondition();

        // act
        var response = condition.Evaluate(null!, gameState, commitEvent);

        // assert
        response.Should().Be(ConditionResponse.NotApplicable);
    }

    [Fact]
    public void GivenRevealCommitmentsCondition_WhenAllPlayersCommitted_ThenReturnsValid()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var commitments = new Dictionary<Player, IGameEvent> { [player1] = new NullGameEvent() };
        var pendingPlayers = new HashSet<Player>();
        var stagedState = new StagedEventsState(artifact, commitments, pendingPlayers);

        var gameState = GameState.New([stagedState]);
        var revealEvent = new RevealCommitmentsEvent();

        var condition = new RevealCommitmentsCondition();

        // act
        var response = condition.Evaluate(null!, gameState, revealEvent);

        // assert
        response.Should().Be(ConditionResponse.Valid);
    }

    [Fact]
    public void GivenRevealCommitmentsCondition_WhenNotAllPlayersCommitted_ThenReturnsInvalid()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player> { player1 };
        var stagedState = new StagedEventsState(artifact, commitments, pendingPlayers);

        var gameState = GameState.New([stagedState]);
        var revealEvent = new RevealCommitmentsEvent();

        var condition = new RevealCommitmentsCondition();

        // act
        var response = condition.Evaluate(null!, gameState, revealEvent);

        // assert
        response.Should().Be(ConditionResponse.Invalid);
    }

    [Fact]
    public void GivenCommitmentPhaseActiveCondition_WhenStagedStateExistsAndNotComplete_ThenReturnsValid()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var pendingPlayers = new HashSet<Player> { player1 };
        var stagedState = new StagedEventsState(artifact, new Dictionary<Player, IGameEvent>(), pendingPlayers);

        var gameState = GameState.New([stagedState]);

        var condition = new CommitmentPhaseActiveCondition();

        // act
        var response = condition.Evaluate(gameState);

        // assert
        response.Should().Be(ConditionResponse.Valid);
    }

    [Fact]
    public void GivenCommitmentPhaseActiveCondition_WhenAllPlayersCommitted_ThenReturnsIgnore()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var commitments = new Dictionary<Player, IGameEvent> { [player1] = new NullGameEvent() };
        var pendingPlayers = new HashSet<Player>();
        var stagedState = new StagedEventsState(artifact, commitments, pendingPlayers);

        var gameState = GameState.New([stagedState]);

        var condition = new CommitmentPhaseActiveCondition();

        // act
        var response = condition.Evaluate(gameState);

        // assert
        response.Result.Should().Be(ConditionResult.Ignore);
    }

    [Fact]
    public void GivenAllPlayersCommittedCondition_WhenComplete_ThenReturnsValid()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var commitments = new Dictionary<Player, IGameEvent> { [player1] = new NullGameEvent() };
        var pendingPlayers = new HashSet<Player>();
        var stagedState = new StagedEventsState(artifact, commitments, pendingPlayers);

        var gameState = GameState.New([stagedState]);

        var condition = new AllPlayersCommittedCondition();

        // act
        var response = condition.Evaluate(gameState);

        // assert
        response.Should().Be(ConditionResponse.Valid);
    }

    [Fact]
    public void GivenAllPlayersCommittedCondition_WhenNotComplete_ThenReturnsIgnore()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var pendingPlayers = new HashSet<Player> { player1 };
        var stagedState = new StagedEventsState(artifact, new Dictionary<Player, IGameEvent>(), pendingPlayers);

        var gameState = GameState.New([stagedState]);

        var condition = new AllPlayersCommittedCondition();

        // act
        var response = condition.Evaluate(gameState);

        // assert
        response.Result.Should().Be(ConditionResult.Ignore);
    }
}
