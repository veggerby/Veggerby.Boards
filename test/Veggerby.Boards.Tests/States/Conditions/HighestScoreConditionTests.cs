using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.States.Conditions;

public class HighestScoreConditionTests
{
    [Fact]
    public void Evaluate_Should_Return_Valid_When_Target_Reached()
    {
        // arrange
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var players = new[] { player1, player2 };

        int ScoreGetter(GameState state, Player player)
        {
            if (player == player1)
            {
                return 100;
            }

            return 50;
        }

        var condition = new HighestScoreCondition(players, ScoreGetter, 100);
        var state = GameState.New(Array.Empty<IArtifactState>());

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void Evaluate_Should_Return_Ignore_When_Target_Not_Reached()
    {
        // arrange
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var players = new[] { player1, player2 };

        int ScoreGetter(GameState state, Player player)
        {
            if (player == player1)
            {
                return 50;
            }

            return 30;
        }

        var condition = new HighestScoreCondition(players, ScoreGetter, 100);
        var state = GameState.New(Array.Empty<IArtifactState>());

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
        result.Reason.Should().Be("Target score not reached");
    }

    [Fact]
    public void Evaluate_Should_Return_Ignore_When_No_Players()
    {
        // arrange
        var players = Array.Empty<Player>();

        int ScoreGetter(GameState state, Player player) => 0;

        var condition = new HighestScoreCondition(players, ScoreGetter, 100);
        var state = GameState.New(Array.Empty<IArtifactState>());

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
        result.Reason.Should().Be("No players in game");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Players_Null()
    {
        // arrange
        int ScoreGetter(GameState state, Player player) => 0;

        // act
        var act = () => new HighestScoreCondition(null!, ScoreGetter, 100);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_ScoreGetter_Null()
    {
        // arrange
        var players = new[] { new Player("player-1") };

        // act
        var act = () => new HighestScoreCondition(players, null!, 100);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}
