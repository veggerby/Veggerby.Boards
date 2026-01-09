using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Examples.SealedBidAuction;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Examples.SealedBidAuction;

/// <summary>
/// Integration tests for sealed-bid auction example demonstrating simultaneous commitment/reveal mechanics.
/// </summary>
public class SealedBidAuctionIntegrationTests
{
    [Fact]
    public void GivenSealedBidAuction_WhenClearWinner_ThenHighestBidderWins()
    {
        // arrange
        var builder = new SealedBidAuctionGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");
        var player3 = progress.Game.Players.First(p => p.Id == "player-3");

        // Initialize staged events state
        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2, player3 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        // Player 1 bids 100
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new PlaceBidEvent(player1, 100)));

        // Player 2 bids 150
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new PlaceBidEvent(player2, 150)));

        // Player 3 bids 75
        progress = progress.HandleEvent(
            new CommitActionEvent(player3, new PlaceBidEvent(player3, 75)));

        // Reveal bids
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        progress.State.GetStates<StagedEventsState>().Should().BeEmpty();

        var bids = progress.State.GetStates<PlayerBidState>().ToList();
        bids.Should().HaveCount(3);

        var outcome = AuctionOutcome.DetermineWinner(progress.State);
        outcome.Winner.Should().Be(player2);
        outcome.WinningBid.Should().Be(150);
        outcome.TerminalCondition.Should().Be("HighestBid");
        outcome.PlayerResults.Should().HaveCount(3);

        var winnerResult = outcome.PlayerResults[0];
        winnerResult.Player.Should().Be(player2);
        winnerResult.Outcome.Should().Be(OutcomeType.Win);
        winnerResult.Rank.Should().Be(1);
        winnerResult.Metrics!["BidAmount"].Should().Be(150);
    }

    [Fact]
    public void GivenSealedBidAuction_WhenTieBid_ThenWinnerDeterminedByPlayerId()
    {
        // arrange
        var builder = new SealedBidAuctionGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");
        var player3 = progress.Game.Players.First(p => p.Id == "player-3");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2, player3 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        // All players bid the same amount
        progress = progress.HandleEvent(
            new CommitActionEvent(player3, new PlaceBidEvent(player3, 100)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new PlaceBidEvent(player1, 100)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new PlaceBidEvent(player2, 100)));

        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome = AuctionOutcome.DetermineWinner(progress.State);

        // Deterministic tie-breaking: player-1 wins (alphabetically first ID)
        outcome.Winner.Should().Be(player1);
        outcome.WinningBid.Should().Be(100);

        // All players should have rank 1 (tied for first)
        outcome.PlayerResults.Should().AllSatisfy(r => r.Rank.Should().Be(1));

        // But only player-1 is marked as winner
        outcome.PlayerResults.Should().ContainSingle(r => r.Outcome == OutcomeType.Win);
        outcome.PlayerResults.First(r => r.Outcome == OutcomeType.Win).Player.Should().Be(player1);
    }

    [Fact]
    public void GivenSealedBidAuction_WhenDifferentBids_ThenRankedCorrectly()
    {
        // arrange
        var builder = new SealedBidAuctionGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");
        var player3 = progress.Game.Players.First(p => p.Id == "player-3");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2, player3 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new PlaceBidEvent(player1, 50)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new PlaceBidEvent(player2, 200)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player3, new PlaceBidEvent(player3, 100)));

        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome = AuctionOutcome.DetermineWinner(progress.State);

        outcome.Winner.Should().Be(player2);
        outcome.WinningBid.Should().Be(200);

        // Verify ranking
        var results = outcome.PlayerResults;
        results[0].Player.Should().Be(player2);
        results[0].Rank.Should().Be(1);
        results[0].Outcome.Should().Be(OutcomeType.Win);
        results[0].Metrics!["BidAmount"].Should().Be(200);

        results[1].Player.Should().Be(player3);
        results[1].Rank.Should().Be(2);
        results[1].Outcome.Should().Be(OutcomeType.Loss);
        results[1].Metrics!["BidAmount"].Should().Be(100);

        results[2].Player.Should().Be(player1);
        results[2].Rank.Should().Be(3);
        results[2].Outcome.Should().Be(OutcomeType.Loss);
        results[2].Metrics!["BidAmount"].Should().Be(50);
    }

    [Fact]
    public void GivenSealedBidAuction_WhenBidsInDifferentOrder_ThenSameOutcome()
    {
        // arrange
        // Test determinism: commitment order doesn't affect auction result
        var builder = new SealedBidAuctionGameBuilder();
        var progress1 = builder.Compile();
        var progress2 = builder.Compile();

        var player1_v1 = progress1.Game.Players.First(p => p.Id == "player-1");
        var player2_v1 = progress1.Game.Players.First(p => p.Id == "player-2");
        var player3_v1 = progress1.Game.Players.First(p => p.Id == "player-3");

        var player1_v2 = progress2.Game.Players.First(p => p.Id == "player-1");
        var player2_v2 = progress2.Game.Players.First(p => p.Id == "player-2");
        var player3_v2 = progress2.Game.Players.First(p => p.Id == "player-3");

        // Setup first game
        var stagedArtifact1 = new StagedEventsArtifact("staged-events");
        var stagedState1 = new StagedEventsState(
            stagedArtifact1,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            new HashSet<Player> { player1_v1, player2_v1, player3_v1 });
        progress1 = new GameProgress(progress1.Engine, progress1.State.Next([stagedState1]), null);

        // Setup second game
        var stagedArtifact2 = new StagedEventsArtifact("staged-events");
        var stagedState2 = new StagedEventsState(
            stagedArtifact2,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            new HashSet<Player> { player1_v2, player2_v2, player3_v2 });
        progress2 = new GameProgress(progress2.Engine, progress2.State.Next([stagedState2]), null);

        // act
        // Version 1: Commit in order 1, 2, 3
        progress1 = progress1.HandleEvent(
            new CommitActionEvent(player1_v1, new PlaceBidEvent(player1_v1, 100)));
        progress1 = progress1.HandleEvent(
            new CommitActionEvent(player2_v1, new PlaceBidEvent(player2_v1, 150)));
        progress1 = progress1.HandleEvent(
            new CommitActionEvent(player3_v1, new PlaceBidEvent(player3_v1, 75)));
        progress1 = progress1.HandleEvent(new RevealCommitmentsEvent());

        // Version 2: Commit in reverse order 3, 2, 1
        progress2 = progress2.HandleEvent(
            new CommitActionEvent(player3_v2, new PlaceBidEvent(player3_v2, 75)));
        progress2 = progress2.HandleEvent(
            new CommitActionEvent(player2_v2, new PlaceBidEvent(player2_v2, 150)));
        progress2 = progress2.HandleEvent(
            new CommitActionEvent(player1_v2, new PlaceBidEvent(player1_v2, 100)));
        progress2 = progress2.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome1 = AuctionOutcome.DetermineWinner(progress1.State);
        var outcome2 = AuctionOutcome.DetermineWinner(progress2.State);

        outcome1.Winner.Id.Should().Be(outcome2.Winner.Id);
        outcome1.WinningBid.Should().Be(outcome2.WinningBid);
        outcome1.PlayerResults.Should().HaveSameCount(outcome2.PlayerResults);
    }

    [Fact]
    public void GivenSealedBidAuction_WhenNegativeBid_ThenCommitSucceedsButApplicationFails()
    {
        // arrange
        var builder = new SealedBidAuctionGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");
        var player3 = progress.Game.Players.First(p => p.Id == "player-3");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2, player3 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        // Commit succeeds (validation happens on reveal)
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new PlaceBidEvent(player1, -50)));

        // assert
        // Commitment is recorded
        var staged = progress.State.GetStates<StagedEventsState>().Single();
        staged.Commitments.Should().ContainKey(player1);
        staged.PendingPlayers.Should().NotContain(player1);

        // Complete the rest of commitments
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new PlaceBidEvent(player2, 10)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player3, new PlaceBidEvent(player3, 5)));

        // Reveal will throw when attempting to apply the invalid bid
        var reveal = () => progress.HandleEvent(new RevealCommitmentsEvent());
        reveal.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void GivenSealedBidAuction_WhenZeroBid_ThenAccepted()
    {
        // arrange
        var builder = new SealedBidAuctionGameBuilder();
        var progress = builder.Compile();

        var player1 = progress.Game.Players.First(p => p.Id == "player-1");
        var player2 = progress.Game.Players.First(p => p.Id == "player-2");
        var player3 = progress.Game.Players.First(p => p.Id == "player-3");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { player1, player2, player3 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        progress = progress.HandleEvent(
            new CommitActionEvent(player1, new PlaceBidEvent(player1, 0)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player2, new PlaceBidEvent(player2, 10)));
        progress = progress.HandleEvent(
            new CommitActionEvent(player3, new PlaceBidEvent(player3, 5)));
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var outcome = AuctionOutcome.DetermineWinner(progress.State);
        outcome.Winner.Should().Be(player2);
        outcome.AllBids[player1].Should().Be(0);
    }
}
