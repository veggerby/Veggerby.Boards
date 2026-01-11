using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.States.Conditions;

public class FirstToScoreConditionTests
{
    [Fact]
    public void Evaluate_Should_Return_Valid_When_Player_Reaches_Target()
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

        var condition = new FirstToScoreCondition(players, ScoreGetter, 100);
        var state = GameState.New(Array.Empty<IArtifactState>());

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void Evaluate_Should_Return_Ignore_When_No_Player_Reaches_Target()
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

        var condition = new FirstToScoreCondition(players, ScoreGetter, 100);
        var state = GameState.New(Array.Empty<IArtifactState>());

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
        result.Reason.Should().Be("No player reached target");
    }

    [Fact]
    public void Evaluate_Should_Return_Valid_For_First_Player_Meeting_Target()
    {
        // arrange
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var players = new[] { player1, player2 };

        int ScoreGetter(GameState state, Player player)
        {
            return 100;
        }

        var condition = new FirstToScoreCondition(players, ScoreGetter, 100);
        var state = GameState.New(Array.Empty<IArtifactState>());

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Players_Null()
    {
        // arrange
        int ScoreGetter(GameState state, Player player) => 0;

        // act
        var act = () => new FirstToScoreCondition(null!, ScoreGetter, 100);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_ScoreGetter_Null()
    {
        // arrange
        var players = new[] { new Player("player-1") };

        // act
        var act = () => new FirstToScoreCondition(players, null!, 100);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}
