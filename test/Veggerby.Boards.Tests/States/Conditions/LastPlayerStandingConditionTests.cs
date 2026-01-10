using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.States.Conditions;

public class LastPlayerStandingConditionTests
{
    [Fact]
    public void Evaluate_Should_Return_Valid_When_One_Player_Remains()
    {
        // arrange
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var players = new[] { player1, player2 };

        var condition = new LastPlayerStandingCondition(players);
        var state = GameState.New(new IArtifactState[]
        {
            new PlayerEliminatedState(player2, "Defeated")
        });

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void Evaluate_Should_Return_Ignore_When_Multiple_Players_Remain()
    {
        // arrange
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var players = new[] { player1, player2 };

        var condition = new LastPlayerStandingCondition(players);
        var state = GameState.New(Array.Empty<IArtifactState>());

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
        result.Reason.Should().Be("Multiple players remaining");
    }

    [Fact]
    public void Evaluate_Should_Return_Ignore_When_All_Players_Eliminated()
    {
        // arrange
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var players = new[] { player1, player2 };

        var condition = new LastPlayerStandingCondition(players);
        var state = GameState.New(new IArtifactState[]
        {
            new PlayerEliminatedState(player1, "Defeated"),
            new PlayerEliminatedState(player2, "Defeated")
        });

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
        result.Reason.Should().Be("No players remaining");
    }

    [Fact]
    public void Evaluate_Should_Handle_Three_Players_With_Two_Eliminated()
    {
        // arrange
        var player1 = new Player("player-1");
        var player2 = new Player("player-2");
        var player3 = new Player("player-3");
        var players = new[] { player1, player2, player3 };

        var condition = new LastPlayerStandingCondition(players);
        var state = GameState.New(new IArtifactState[]
        {
            new PlayerEliminatedState(player2, "Bankruptcy"),
            new PlayerEliminatedState(player3, "Defeated")
        });

        // act
        var result = condition.Evaluate(state);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Players_Null()
    {
        // arrange & act
        var act = () => new LastPlayerStandingCondition(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}
