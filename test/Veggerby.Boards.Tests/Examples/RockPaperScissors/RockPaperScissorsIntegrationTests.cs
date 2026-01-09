using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Examples.RockPaperScissors;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Examples.RockPaperScissors;

/// <summary>
/// Integration tests for Rock-Paper-Scissors example demonstrating simultaneous commitment/reveal mechanics.
/// </summary>
public class RockPaperScissorsIntegrationTests
{
    [Fact]
    public void GivenRockPaperScissorsGame_WhenPlayer1RockPlayer2Scissors_ThenPlayer1Wins()
    {
        // arrange
        var builder = new RockPaperScissorsGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");

        // Initialize staged events state
        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        // Player 1 commits Rock
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new SelectChoiceEvent(player1, Choice.Rock)));

        // Player 2 commits Scissors
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new SelectChoiceEvent(player2, Choice.Scissors)));

        // Verify both commitments are recorded
        var staged = progress.State.GetStates<StagedEventsState>().Single();
        staged.IsComplete.Should().BeTrue();
        staged.Commitments.Should().HaveCount(2);

        // Reveal commitments
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        // Staged state should be cleared
        progress.State.GetStates<StagedEventsState>().Should().BeEmpty();

        // Both players should have their choices recorded
        var choices = progress.State.GetStates<PlayerChoiceState>().ToList();
        choices.Should().HaveCount(2);

        var player1Choice = choices.First(c => c.Artifact.Equals(player1));
        var player2Choice = choices.First(c => c.Artifact.Equals(player2));

        player1Choice.Choice.Should().Be(Choice.Rock);
        player2Choice.Choice.Should().Be(Choice.Scissors);

        // Determine outcome
        var outcome = RockPaperScissorsOutcome.DetermineWinner(progress.State, player1, player2);
        outcome.Winner.Should().Be(player1);
        outcome.IsTie.Should().BeFalse();
        outcome.TerminalCondition.Should().Be("Winner");
        outcome.PlayerResults.Should().HaveCount(2);
        outcome.PlayerResults[0].Player.Should().Be(player1);
        outcome.PlayerResults[0].Outcome.Should().Be(OutcomeType.Win);
        outcome.PlayerResults[0].Rank.Should().Be(1);
        outcome.PlayerResults[1].Player.Should().Be(player2);
        outcome.PlayerResults[1].Outcome.Should().Be(OutcomeType.Loss);
        outcome.PlayerResults[1].Rank.Should().Be(2);
    }

    [Fact]
    public void GivenRockPaperScissorsGame_WhenBothPlayersChoosePaper_ThenTie()
    {
        // arrange
        var builder = new RockPaperScissorsGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new SelectChoiceEvent(player1, Choice.Paper)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new SelectChoiceEvent(player2, Choice.Paper)));
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome = RockPaperScissorsOutcome.DetermineWinner(progress.State, player1, player2);
        outcome.Winner.Should().BeNull();
        outcome.IsTie.Should().BeTrue();
        outcome.TerminalCondition.Should().Be("Draw");
        outcome.PlayerResults.Should().HaveCount(2);
        outcome.PlayerResults.Should().AllSatisfy(r =>
        {
            r.Outcome.Should().Be(OutcomeType.Draw);
            r.Rank.Should().Be(1);
        });
    }

    [Fact]
    public void GivenRockPaperScissorsGame_WhenPlayer1PaperPlayer2Rock_ThenPlayer1Wins()
    {
        // arrange
        var builder = new RockPaperScissorsGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new SelectChoiceEvent(player1, Choice.Paper)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new SelectChoiceEvent(player2, Choice.Rock)));
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome = RockPaperScissorsOutcome.DetermineWinner(progress.State, player1, player2);
        outcome.Winner.Should().Be(player1);
        outcome.Player1Choice.Should().Be(Choice.Paper);
        outcome.Player2Choice.Should().Be(Choice.Rock);
    }

    [Fact]
    public void GivenRockPaperScissorsGame_WhenPlayer1ScissorsPlayer2Paper_ThenPlayer1Wins()
    {
        // arrange
        var builder = new RockPaperScissorsGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new SelectChoiceEvent(player1, Choice.Scissors)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new SelectChoiceEvent(player2, Choice.Paper)));
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome = RockPaperScissorsOutcome.DetermineWinner(progress.State, player1, player2);
        outcome.Winner.Should().Be(player1);
        outcome.Player1Choice.Should().Be(Choice.Scissors);
        outcome.Player2Choice.Should().Be(Choice.Paper);
    }

    [Fact]
    public void GivenRockPaperScissorsGame_WhenCommitmentsInReverseOrder_ThenSameOutcome()
    {
        // arrange
        // This test verifies determinism: commitment order doesn't affect outcome
        var builder = new RockPaperScissorsGameBuilder();
        var progress1 = builder.Compile();
        var progress2 = builder.Compile();

        var player1_v1 = progress1.Game.Players.First(p => p.Id == "player-1");
        var player2_v1 = progress1.Game.Players.First(p => p.Id == "player-2");
        var player1_v2 = progress2.Game.Players.First(p => p.Id == "player-1");
        var player2_v2 = progress2.Game.Players.First(p => p.Id == "player-2");

        // Setup first game
        var stagedArtifact1 = new StagedEventsArtifact("staged-events");
        var stagedState1 = new StagedEventsState(
            stagedArtifact1,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            new HashSet<Player> { player1_v1, player2_v1 });
        progress1 = new GameProgress(progress1.Engine, progress1.State.Next([stagedState1]), null);

        // Setup second game
        var stagedArtifact2 = new StagedEventsArtifact("staged-events");
        var stagedState2 = new StagedEventsState(
            stagedArtifact2,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            new HashSet<Player> { player1_v2, player2_v2 });
        progress2 = new GameProgress(progress2.Engine, progress2.State.Next([stagedState2]), null);

        // act
        // Version 1: Player 1 commits first
        progress1 = progress1.HandleEvent(
            new CommitActionEvent(player1_v1, new SelectChoiceEvent(player1_v1, Choice.Rock)));
        progress1 = progress1.HandleEvent(
            new CommitActionEvent(player2_v1, new SelectChoiceEvent(player2_v1, Choice.Scissors)));
        progress1 = progress1.HandleEvent(new RevealCommitmentsEvent());

        // Version 2: Player 2 commits first
        progress2 = progress2.HandleEvent(
            new CommitActionEvent(player2_v2, new SelectChoiceEvent(player2_v2, Choice.Scissors)));
        progress2 = progress2.HandleEvent(
            new CommitActionEvent(player1_v2, new SelectChoiceEvent(player1_v2, Choice.Rock)));
        progress2 = progress2.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome1 = RockPaperScissorsOutcome.DetermineWinner(progress1.State, player1_v1, player2_v1);
        var outcome2 = RockPaperScissorsOutcome.DetermineWinner(progress2.State, player1_v2, player2_v2);

        outcome1.Winner!.Id.Should().Be(outcome2.Winner!.Id);
        outcome1.Player1Choice.Should().Be(outcome2.Player1Choice);
        outcome1.Player2Choice.Should().Be(outcome2.Player2Choice);
    }

    [Fact]
    public void GivenRockPaperScissorsGame_WhenAttemptingToCommitTwice_ThenSecondCommitRejected()
    {
        // arrange
        var builder = new RockPaperScissorsGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new SelectChoiceEvent(player1, Choice.Rock)));

        // Attempt to commit again for the same player
        var secondCommit = () => progress.HandleEvent(
            new CommitActionEvent(player1, new SelectChoiceEvent(player1, Choice.Paper)));

        // assert
        secondCommit.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void GivenRockPaperScissorsGame_WhenRevealingBeforeAllCommitted_ThenRevealHasNoEffect()
    {
        // arrange
        var builder = new RockPaperScissorsGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        // Only player 1 commits
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new SelectChoiceEvent(player1, Choice.Rock)));

        var stateBefore = progress.State;

        // Attempt to reveal before player 2 commits
        var progressAfterReveal = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        // State should be unchanged (reveal has no effect)
        progressAfterReveal.State.Should().Be(stateBefore);

        // Staged events should still exist
        var staged = progressAfterReveal.State.GetStates<StagedEventsState>().Single();
        staged.IsComplete.Should().BeFalse();
        staged.PendingPlayers.Should().Contain(player2);
    }
}
