using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Utilities.Scoring;

namespace Veggerby.Boards.Tests.Utilities.Scoring;

public class OutcomeBuilderTests
{
    [Fact]
    public void WithWinner_Should_Add_Winner_At_Rank_One()
    {
        // arrange
        var builder = new OutcomeBuilder();
        var player = new Player("player-1");

        // act
        var outcome = builder.WithWinner(player).Build();

        // assert
        outcome.PlayerResults.Should().HaveCount(1);
        outcome.PlayerResults[0].Player.Should().Be(player);
        outcome.PlayerResults[0].Rank.Should().Be(1);
        outcome.PlayerResults[0].Outcome.Should().Be(OutcomeType.Win);
    }

    [Fact]
    public void WithLoser_Should_Add_Loser_With_Next_Rank()
    {
        // arrange
        var builder = new OutcomeBuilder();
        var winner = new Player("player-1");
        var loser = new Player("player-2");

        // act
        var outcome = builder
            .WithWinner(winner)
            .WithLoser(loser)
            .Build();

        // assert
        outcome.PlayerResults.Should().HaveCount(2);
        outcome.PlayerResults[1].Player.Should().Be(loser);
        outcome.PlayerResults[1].Rank.Should().Be(2);
        outcome.PlayerResults[1].Outcome.Should().Be(OutcomeType.Loss);
    }

    [Fact]
    public void WithRankedPlayers_Should_Add_Players_From_Scores()
    {
        // arrange
        var builder = new OutcomeBuilder();
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");

        var scores = new[]
        {
            new PlayerScore { Player = player1, Score = 100, Rank = 1 },
            new PlayerScore { Player = player2, Score = 50, Rank = 2 }
        };

        // act
        var outcome = builder.WithRankedPlayers(scores).Build();

        // assert
        outcome.PlayerResults.Should().HaveCount(2);
        outcome.PlayerResults[0].Player.Should().Be(player1);
        outcome.PlayerResults[0].Rank.Should().Be(1);
        outcome.PlayerResults[0].Outcome.Should().Be(OutcomeType.Win);
        outcome.PlayerResults[0].Metrics.Should().ContainKey("Score");
        outcome.PlayerResults[0].Metrics!["Score"].Should().Be(100);
        outcome.PlayerResults[1].Player.Should().Be(player2);
        outcome.PlayerResults[1].Rank.Should().Be(2);
        outcome.PlayerResults[1].Outcome.Should().Be(OutcomeType.Loss);
    }

    [Fact]
    public void WithTiedPlayers_Should_Add_All_At_Rank_One_With_Draw()
    {
        // arrange
        var builder = new OutcomeBuilder();
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");

        // act
        var outcome = builder.WithTiedPlayers(new[] { player1, player2 }).Build();

        // assert
        outcome.PlayerResults.Should().HaveCount(2);
        outcome.PlayerResults[0].Player.Should().Be(player1);
        outcome.PlayerResults[0].Rank.Should().Be(1);
        outcome.PlayerResults[0].Outcome.Should().Be(OutcomeType.Draw);
        outcome.PlayerResults[1].Player.Should().Be(player2);
        outcome.PlayerResults[1].Rank.Should().Be(1);
        outcome.PlayerResults[1].Outcome.Should().Be(OutcomeType.Draw);
    }

    [Fact]
    public void WithRankedPlayers_Should_Handle_Tied_Scores_As_Draw()
    {
        // arrange
        var builder = new OutcomeBuilder();
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var player3 = new Player("player-3");

        // Create tied scores at rank 1
        var scores = new[]
        {
            new PlayerScore { Player = player1, Score = 100, Rank = 1 },
            new PlayerScore { Player = player2, Score = 100, Rank = 1 },
            new PlayerScore { Player = player3, Score = 50, Rank = 3 }
        };

        // act
        var outcome = builder.WithRankedPlayers(scores).Build();

        // assert
        outcome.PlayerResults.Should().HaveCount(3);
        outcome.PlayerResults[0].Player.Should().Be(player1);
        outcome.PlayerResults[0].Rank.Should().Be(1);
        outcome.PlayerResults[0].Outcome.Should().Be(OutcomeType.Draw);
        outcome.PlayerResults[1].Player.Should().Be(player2);
        outcome.PlayerResults[1].Rank.Should().Be(1);
        outcome.PlayerResults[1].Outcome.Should().Be(OutcomeType.Draw);
        outcome.PlayerResults[2].Player.Should().Be(player3);
        outcome.PlayerResults[2].Rank.Should().Be(3);
        outcome.PlayerResults[2].Outcome.Should().Be(OutcomeType.Loss);
    }

    [Fact]
    public void WithTerminalCondition_Should_Set_Condition()
    {
        // arrange
        var builder = new OutcomeBuilder();

        // act
        var outcome = builder.WithTerminalCondition("Checkmate").Build();

        // assert
        outcome.TerminalCondition.Should().Be("Checkmate");
    }

    [Fact]
    public void Build_Should_Use_Default_Terminal_Condition()
    {
        // arrange
        var builder = new OutcomeBuilder();
        var player = new Player("player-1");

        // act
        var outcome = builder.WithWinner(player).Build();

        // assert
        outcome.TerminalCondition.Should().Be("GameEnded");
    }

    [Fact]
    public void WithRankedPlayers_Should_Throw_When_Scores_Is_Null()
    {
        // arrange
        var builder = new OutcomeBuilder();

        // act
        var act = () => builder.WithRankedPlayers(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithTiedPlayers_Should_Throw_When_Players_Is_Null()
    {
        // arrange
        var builder = new OutcomeBuilder();

        // act
        var act = () => builder.WithTiedPlayers(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Builder_Should_Support_Fluent_Chaining()
    {
        // arrange
        var builder = new OutcomeBuilder();
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");

        // act
        var outcome = builder
            .WithTerminalCondition("Scoring")
            .WithWinner(player1, new Dictionary<string, object> { ["VictoryPoints"] = 100 })
            .WithLoser(player2, new Dictionary<string, object> { ["VictoryPoints"] = 50 })
            .Build();

        // assert
        outcome.TerminalCondition.Should().Be("Scoring");
        outcome.PlayerResults.Should().HaveCount(2);
        outcome.PlayerResults[0].Metrics.Should().ContainKey("VictoryPoints");
        outcome.PlayerResults[1].Metrics.Should().ContainKey("VictoryPoints");
    }
}
