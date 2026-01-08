using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Commitment;

public class StagedEventsStateTests
{
    [Fact]
    public void GivenNewStagedEventsState_WhenNoPendingPlayers_ThenIsCompleteIsTrue()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player>();

        // act
        var state = new StagedEventsState(artifact, commitments, pendingPlayers);

        // assert
        state.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void GivenNewStagedEventsState_WhenPendingPlayersExist_ThenIsCompleteIsFalse()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player> { player1 };

        // act
        var state = new StagedEventsState(artifact, commitments, pendingPlayers);

        // assert
        state.IsComplete.Should().BeFalse();
        state.PendingPlayers.Should().Contain(player1);
    }

    [Fact]
    public void GivenStagedEventsState_WhenAddingCommitment_ThenPlayerRemovedFromPending()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var state = new StagedEventsState(artifact, commitments, pendingPlayers);
        var action = new NullGameEvent();

        // act
        var newState = state.AddCommitment(player1, action);

        // assert
        newState.PendingPlayers.Should().NotContain(player1);
        newState.PendingPlayers.Should().Contain(player2);
        newState.Commitments.Should().ContainKey(player1);
        newState.Commitments[player1].Should().Be(action);
        newState.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void GivenStagedEventsState_WhenAddingAllCommitments_ThenIsCompleteIsTrue()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var state = new StagedEventsState(artifact, commitments, pendingPlayers);
        var action1 = new NullGameEvent();
        var action2 = new NullGameEvent();

        // act
        var state2 = state.AddCommitment(player1, action1);
        var finalState = state2.AddCommitment(player2, action2);

        // assert
        finalState.IsComplete.Should().BeTrue();
        finalState.PendingPlayers.Should().BeEmpty();
        finalState.Commitments.Should().HaveCount(2);
        finalState.Commitments[player1].Should().Be(action1);
        finalState.Commitments[player2].Should().Be(action2);
    }

    [Fact]
    public void GivenStagedEventsState_WhenAddingDuplicateCommitment_ThenThrowsInvalidOperationException()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player> { player1 };
        var state = new StagedEventsState(artifact, commitments, pendingPlayers);
        var action1 = new NullGameEvent();
        var state2 = state.AddCommitment(player1, action1);

        // act & assert
        var act = () => state2.AddCommitment(player1, new NullGameEvent());
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already committed*");
    }

    [Fact]
    public void GivenStagedEventsState_WhenAddingCommitmentForUnexpectedPlayer_ThenThrowsInvalidOperationException()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player> { player1 };
        var state = new StagedEventsState(artifact, commitments, pendingPlayers);

        // act & assert
        var act = () => state.AddCommitment(player2, new NullGameEvent());
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not expected to commit*");
    }

    [Fact]
    public void GivenTwoStagedEventsStates_WhenIdentical_ThenEqualsReturnsTrue()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var action = new NullGameEvent();
        var commitments = new Dictionary<Player, IGameEvent> { [player1] = action };
        var pendingPlayers = new HashSet<Player>();

        var state1 = new StagedEventsState(artifact, commitments, pendingPlayers);
        var state2 = new StagedEventsState(artifact, commitments, pendingPlayers);

        // act & assert
        state1.Equals(state2).Should().BeTrue();
        state1.GetHashCode().Should().Be(state2.GetHashCode());
    }

    [Fact]
    public void GivenTwoStagedEventsStates_WhenDifferentCommitments_ThenEqualsReturnsFalse()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var player1 = new Player("player-1");
        var action1 = new NullGameEvent();
        var action2 = new NullGameEvent();
        var commitments1 = new Dictionary<Player, IGameEvent> { [player1] = action1 };
        var commitments2 = new Dictionary<Player, IGameEvent> { [player1] = action2 };
        var pendingPlayers = new HashSet<Player>();

        var state1 = new StagedEventsState(artifact, commitments1, pendingPlayers);
        var state2 = new StagedEventsState(artifact, commitments2, pendingPlayers);

        // act & assert
        state1.Equals(state2).Should().BeFalse();
    }

    [Fact]
    public void GivenStagedEventsState_ThenVisibilityIsHidden()
    {
        // arrange
        var artifact = new StagedEventsArtifact("staged-events");
        var commitments = new Dictionary<Player, IGameEvent>();
        var pendingPlayers = new HashSet<Player>();
        var state = new StagedEventsState(artifact, commitments, pendingPlayers);

        // act & assert
        state.Visibility.Should().Be(Visibility.Hidden);
    }
}
