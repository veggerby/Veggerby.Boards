using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Utilities.Scoring;

namespace Veggerby.Boards.Tests.Utilities.Scoring;

public class ScoreAccumulatorTests
{
    [Fact]
    public void Constructor_Should_Create_Empty_Accumulator()
    {
        // arrange & act
        var accumulator = new ScoreAccumulator();
        var player = new Player("player-1");

        // assert
        accumulator.GetScore(player).Should().Be(0);
    }

    [Fact]
    public void Add_Should_Accumulate_Points()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player = new Player("player-1");

        // act
        var result = accumulator.Add(player, 10);

        // assert
        result.GetScore(player).Should().Be(10);
    }

    [Fact]
    public void Add_Should_Sum_Multiple_Additions()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player = new Player("player-1");

        // act
        var result = accumulator
            .Add(player, 10)
            .Add(player, 5)
            .Add(player, 3);

        // assert
        result.GetScore(player).Should().Be(18);
    }

    [Fact]
    public void Add_Should_Handle_Negative_Points()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player = new Player("player-1");

        // act
        var result = accumulator
            .Add(player, 10)
            .Add(player, -3);

        // assert
        result.GetScore(player).Should().Be(7);
    }

    [Fact]
    public void Set_Should_Override_Current_Score()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player = new Player("player-1");

        // act
        var result = accumulator
            .Add(player, 10)
            .Set(player, 25);

        // assert
        result.GetScore(player).Should().Be(25);
    }

    [Fact]
    public void GetRankedScores_Should_Order_By_Score_Descending()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var player3 = new Player("player-3");

        accumulator = accumulator
            .Add(player1, 10)
            .Add(player2, 25)
            .Add(player3, 15);

        // act
        var ranked = accumulator.GetRankedScores();

        // assert
        ranked.Should().HaveCount(3);
        ranked[0].Player.Should().Be(player2);
        ranked[0].Score.Should().Be(25);
        ranked[0].Rank.Should().Be(1);
        ranked[1].Player.Should().Be(player3);
        ranked[1].Score.Should().Be(15);
        ranked[1].Rank.Should().Be(2);
        ranked[2].Player.Should().Be(player1);
        ranked[2].Score.Should().Be(10);
        ranked[2].Rank.Should().Be(3);
    }

    [Fact]
    public void GetRankedScores_Should_Handle_Tied_Scores()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var player3 = new Player("player-3");

        accumulator = accumulator
            .Add(player1, 20)
            .Add(player2, 20)
            .Add(player3, 10);

        // act
        var ranked = accumulator.GetRankedScores();

        // assert
        ranked.Should().HaveCount(3);
        ranked[0].Score.Should().Be(20);
        ranked[0].Rank.Should().Be(1);
        ranked[1].Score.Should().Be(20);
        ranked[1].Rank.Should().Be(1);
        ranked[2].Score.Should().Be(10);
        ranked[2].Rank.Should().Be(3);
    }

    [Fact]
    public void GetRankedScores_Should_Return_Empty_For_New_Accumulator()
    {
        // arrange
        var accumulator = new ScoreAccumulator();

        // act
        var ranked = accumulator.GetRankedScores();

        // assert
        ranked.Should().BeEmpty();
    }

    [Fact]
    public void Add_Should_Be_Immutable()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player = new Player("player-1");

        // act
        var result = accumulator.Add(player, 10);

        // assert
        accumulator.GetScore(player).Should().Be(0);
        result.GetScore(player).Should().Be(10);
    }

    [Fact]
    public void Set_Should_Be_Immutable()
    {
        // arrange
        var accumulator = new ScoreAccumulator();
        var player = new Player("player-1");

        accumulator = accumulator.Add(player, 10);

        // act
        var result = accumulator.Set(player, 25);

        // assert
        accumulator.GetScore(player).Should().Be(10);
        result.GetScore(player).Should().Be(25);
    }
}
