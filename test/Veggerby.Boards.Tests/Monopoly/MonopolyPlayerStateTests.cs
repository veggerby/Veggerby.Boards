using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class MonopolyPlayerStateTests
{
    private static Player CreateTestPlayer(string id = "test-player")
    {
        return new Player(id);
    }

    [Fact]
    public void Constructor_WithCash_SetsCash()
    {
        // arrange
        var player = CreateTestPlayer();

        // act
        var state = new MonopolyPlayerState(player, 1500);

        // assert
        state.Cash.Should().Be(1500);
        state.Player.Should().Be(player);
        state.InJail.Should().BeFalse();
        state.JailTurns.Should().Be(0);
        state.HasGetOutOfJailCard.Should().BeFalse();
        state.IsBankrupt.Should().BeFalse();
    }

    [Fact]
    public void WithCash_ReturnsNewStateWithUpdatedCash()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500);

        // act
        var newState = state.WithCash(1000);

        // assert
        newState.Cash.Should().Be(1000);
        state.Cash.Should().Be(1500); // Original unchanged
    }

    [Fact]
    public void AdjustCash_AddsPositiveAmount()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500);

        // act
        var newState = state.AdjustCash(200);

        // assert
        newState.Cash.Should().Be(1700);
    }

    [Fact]
    public void AdjustCash_SubtractsNegativeAmount()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500);

        // act
        var newState = state.AdjustCash(-200);

        // assert
        newState.Cash.Should().Be(1300);
    }

    [Fact]
    public void GoToJail_SetsInJailAndResetsDoubles()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500, consecutiveDoubles: 2);

        // act
        var newState = state.GoToJail();

        // assert
        newState.InJail.Should().BeTrue();
        newState.JailTurns.Should().Be(0);
        newState.ConsecutiveDoubles.Should().Be(0);
        newState.Cash.Should().Be(1500); // Cash unchanged
    }

    [Fact]
    public void ReleaseFromJail_ResetsJailState()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500, inJail: true, jailTurns: 2);

        // act
        var newState = state.ReleaseFromJail();

        // assert
        newState.InJail.Should().BeFalse();
        newState.JailTurns.Should().Be(0);
    }

    [Fact]
    public void IncrementJailTurns_IncrementsTurns()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500, inJail: true, jailTurns: 1);

        // act
        var newState = state.IncrementJailTurns();

        // assert
        newState.JailTurns.Should().Be(2);
    }

    [Fact]
    public void WithGetOutOfJailCard_SetsCardStatus()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500);

        // act
        var newState = state.WithGetOutOfJailCard(true);

        // assert
        newState.HasGetOutOfJailCard.Should().BeTrue();
    }

    [Fact]
    public void MarkBankrupt_SetsBankruptAndResetsCash()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, -100, inJail: true);

        // act
        var newState = state.MarkBankrupt();

        // assert
        newState.IsBankrupt.Should().BeTrue();
        newState.Cash.Should().Be(0);
        newState.InJail.Should().BeFalse();
    }

    [Fact]
    public void WithConsecutiveDoubles_SetsDoublesCount()
    {
        // arrange
        var player = CreateTestPlayer();
        var state = new MonopolyPlayerState(player, 1500);

        // act
        var newState = state.WithConsecutiveDoubles(2);

        // assert
        newState.ConsecutiveDoubles.Should().Be(2);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // arrange
        var player = CreateTestPlayer();
        var state1 = new MonopolyPlayerState(player, 1500);
        var state2 = new MonopolyPlayerState(player, 1500);

        // act
        var result = state1.Equals((IArtifactState)state2);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCash_ReturnsFalse()
    {
        // arrange
        var player = CreateTestPlayer();
        var state1 = new MonopolyPlayerState(player, 1500);
        var state2 = new MonopolyPlayerState(player, 1000);

        // act
        var result = state1.Equals((IArtifactState)state2);

        // assert
        result.Should().BeFalse();
    }
}
